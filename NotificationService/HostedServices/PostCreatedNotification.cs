namespace NotificationService.HostedServices;

public record PostCreatedNotification(long PostId, string PostText, long AuthorUserId, long RecipientId);
