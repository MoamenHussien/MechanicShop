public readonly record struct Error
{
    public string code { get;}
    public string  Description { get; }
    public ErrorKind Kind {get;}

    private Error(string Code , string Description,ErrorKind Kind)
    {
       this.code = Code;
       this.Description = Description;
       this.Kind = Kind;
    }
    
    public static Error Failure (string Code = nameof(Failure) , string Description= "General Failure") => new Error(Code,Description,ErrorKind.Failure);
    public static Error Unexpected (string Code= nameof(Unexpected),string description = "Unexpected Error")=> new Error(Code,description,ErrorKind.Unexpected);
    public static Error Validation (string code = nameof(Validation),string description="Validation Error")=> new Error(code, description, ErrorKind.Validation);
    public static Error Conflict (string code = nameof(Conflict), string description =  "Conflict Error")=> new Error(code,description,ErrorKind.Conflict);
    public static Error NotFound (string code = nameof(NotFound), string description =  "NotFound Error")=> new Error(code,description,ErrorKind.NotFound);
    public static Error Unauthorized (string code = nameof(Unauthorized), string description =  "Unauthorized Error")=> new Error(code,description,ErrorKind.Unauthorized);
    public static Error Forbidden (string code = nameof(Forbidden), string description =  "Forbidden Error")=> new Error(code,description,ErrorKind.Forbidden);
    


}