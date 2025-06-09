using Azure.Messaging.ServiceBus;
using Emailing.Service.Data.Repositories;
using Emailing.Service.Models;
using MailKit.Net.Smtp;
using MimeKit;
using System.Net.Mail;
using System.Text.Json;

namespace Emailing.Service.Services
{
	public class EmailingBackgroundService : BackgroundService
	{
		private readonly ServiceBusClient _client;
		private readonly ServiceBusProcessor _confirmationProcessor;
		private readonly ServiceBusProcessor _reminderProcessor;
		private readonly UserRepository _userRepository;
		private readonly ILogger<EmailingBackgroundService> _logger;

		public EmailingBackgroundService(ServiceBusClient client, UserRepository userRepository, ILogger<EmailingBackgroundService> logger)
		{
			_client = client;
			_userRepository = userRepository;
			_logger = logger;

			_confirmationProcessor = _client.CreateProcessor("confirmation-queue", new ServiceBusProcessorOptions());
			_reminderProcessor = _client.CreateProcessor("reminder-queue", new ServiceBusProcessorOptions());
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_confirmationProcessor.ProcessMessageAsync += ProcessConfirmationHandler;
			_confirmationProcessor.ProcessErrorAsync += ErrorHandler;

			_reminderProcessor.ProcessMessageAsync += ProcessReminderHandler;
			_reminderProcessor.ProcessErrorAsync += ErrorHandler;

			await _confirmationProcessor.StartProcessingAsync(stoppingToken);
			await _reminderProcessor.StartProcessingAsync(stoppingToken);

			_logger.LogInformation("Emailing background service started");

			await Task.Delay(Timeout.Infinite, stoppingToken);

			await _confirmationProcessor.StopProcessingAsync();
			await _reminderProcessor.StopProcessingAsync();
		}

		private async Task ProcessConfirmationHandler(ProcessMessageEventArgs args)
		{
			var body = args.Message.Body.ToString();
			var info = JsonSerializer.Deserialize<ReservationConfirmationEmailInfo>(body);
			await SendReservationConfirmationToUser(info!);
			await args.CompleteMessageAsync(args.Message);
		}

		private async Task ProcessReminderHandler(ProcessMessageEventArgs args)
		{
			var body = args.Message.Body.ToString();
			var info = JsonSerializer.Deserialize<ReservationConfirmationEmailInfo>(body);
			await SendReminderEmailToUser(info!);
			await args.CompleteMessageAsync(args.Message);
		}

		private Task ErrorHandler(ProcessErrorEventArgs args)
		{
			_logger.LogError(args.Exception, "Service Bus error");
			return Task.CompletedTask;
		}

		private async Task SendReservationConfirmationToUser(ReservationConfirmationEmailInfo info)
		{
			var user = await _userRepository.GetUserAsync(info.UserId);
			if (user == null)
			{
				_logger.LogWarning("No user found with ID {UserId}", info.UserId);
				return;
			}

			var message = new MailMessage
			{
				From = new MailAddress("borisov.petar02@gmail.com"),
				Subject = "Reservation Confirmation",
				Body = $"Hello {user.FirstName},\n\nYour reservation for {info.Service} at {info.Saloon} is confirmed for {info.StartTime}.\n\nThank you!"
			};
			message.To.Add(user.Email);

			await SendEmailAsync(message, user.Email);
		}

		private async Task SendReminderEmailToUser(ReservationConfirmationEmailInfo info)
		{
			var user = await _userRepository.GetUserAsync(info.UserId);
			if (user == null)
			{
				_logger.LogWarning("No user found with ID {UserId}", info.UserId);
				return;
			}

			var message = new MailMessage
			{
				From = new MailAddress("borisov.petar02@gmail.com"),
				Subject = "Reservation Reminder",
				Body = $"Hello {user.FirstName},\n\nThis is a reminder for your reservation for {info.Service} at {info.Saloon} scheduled for {info.StartTime}.\n\nSee you soon!"
			};
			message.To.Add(user.Email);

			await SendEmailAsync(message, user.Email);
		}

		private async Task SendEmailAsync(MailMessage message, string recipient)
		{
			var mimeMessage = new MimeMessage();

			mimeMessage.From.Add(new MailboxAddress(message.From.DisplayName, message.From.Address));
			mimeMessage.To.Add(new MailboxAddress("", recipient));
			mimeMessage.Subject = message.Subject;

			mimeMessage.Body = new TextPart("plain") { Text = message.Body };

			using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
			try
			{
				await smtpClient.ConnectAsync("smtp.gmail.com", 465, true);
				await smtpClient.AuthenticateAsync("borisov.petar02@gmail.com", "fagb uvzw rass wppt");
				await smtpClient.SendAsync(mimeMessage);
				await smtpClient.DisconnectAsync(true);
				_logger.LogInformation("Email sent to {Recipient}", recipient);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send email to {Recipient}", recipient);
			}
		}
	}

}
