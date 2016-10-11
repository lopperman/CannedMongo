namespace CS.BO.Interfaces
{
    public interface IUserElementConstraint
    {
        object MinValue { get; set; }
        object MaxValue { get; set; }
        int MinLength { get; set; }
        int MaxLength { get; set; }
        bool Required { get; set; }
    }
}
