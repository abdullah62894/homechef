using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using HomeChef.Domain.Messages;

namespace HomeChef.Application.Features.Messages;

public interface IContactMessageService
{
    Task<ContactMessage> SubmitAsync(string name, string phone, string email, string message);
    Task<IReadOnlyList<ContactMessage>> ListAsync(int page, int pageSize);
    Task<ContactMessage?> GetByIdAsync(Guid id);
    Task MarkAsReadAsync(Guid id);
    Task UpdateStatusAsync(Guid id, string status);
    Task<int> GetUnreadCountAsync();
}

public sealed class ContactMessageService : IContactMessageService
{
    private readonly IContactMessageRepository _repository;

    public ContactMessageService(IContactMessageRepository repository) => _repository = repository;

    public async Task<ContactMessage> SubmitAsync(string name, string phone, string email, string message)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone) ||
            string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            throw new BusinessException(ErrorCodes.ContactMessageInvalid, "All fields are required.");

        var contactMessage = new ContactMessage
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Phone = phone.Trim(),
            Email = email.Trim(),
            Message = message.Trim(),
            IsRead = false,
            Status = "New",
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _repository.AddAsync(contactMessage);
        return contactMessage;
    }

    public Task<IReadOnlyList<ContactMessage>> ListAsync(int page, int pageSize)
        => _repository.ListAsync(page, pageSize);

    public Task<ContactMessage?> GetByIdAsync(Guid id)
        => _repository.GetByIdAsync(id);

    public async Task MarkAsReadAsync(Guid id)
    {
        var msg = await _repository.GetByIdAsync(id)
            ?? throw new BusinessException(ErrorCodes.MessageNotFound, "Message not found.");
        msg.IsRead = true;
        msg.ReadAtUtc = DateTime.UtcNow;
        if (msg.Status == "New") msg.Status = "Read";
        await _repository.UpdateAsync(msg);
    }

    public async Task UpdateStatusAsync(Guid id, string status)
    {
        var msg = await _repository.GetByIdAsync(id)
            ?? throw new BusinessException(ErrorCodes.MessageNotFound, "Message not found.");
        msg.Status = status;
        if (status is "Read" or "Resolved" && !msg.IsRead)
        {
            msg.IsRead = true;
            msg.ReadAtUtc = DateTime.UtcNow;
        }
        await _repository.UpdateAsync(msg);
    }

    public Task<int> GetUnreadCountAsync() => _repository.GetUnreadCountAsync();
}
