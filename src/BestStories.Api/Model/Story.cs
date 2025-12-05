namespace BestStories.Api.Model;

public record Story(int Id, string? By, long Time, string? Title, string? Url, int? Score, int? Descendants, string? Type);
