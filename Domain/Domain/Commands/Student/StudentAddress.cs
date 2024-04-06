namespace Domain.Commands.Student
{
    /// <summary>
    /// 地址
    /// </summary>
    public class StudentAddress
    {
        public string? Province { get; set; }
        public string? City { get; set; }
        public required string County { get; set; }
        public required string Street { get; set; }
    }
}
