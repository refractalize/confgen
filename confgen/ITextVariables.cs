namespace Confgen {
    internal interface ITextVariables {
        string this[string name] { get; set; }
        ITextVariables InnerFrame { get; }
    }
}