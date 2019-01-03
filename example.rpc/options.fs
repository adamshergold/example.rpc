namespace Example.Rpc

open Microsoft.Extensions.Logging 

open Example.Messaging 

type MessagingOptions = {
    Recipient : IRecipient 
    TimeoutMilliseconds : int 
    Label : string 
}
with 
    static member Default( recipient ) = {
        Recipient = recipient
        TimeoutMilliseconds = 250
        Label = "" 
    }
    
type ClientOptions = {
    Logger : ILogger option 
    Messaging : MessagingOptions option
}    
with 
    static member Default : ClientOptions = { 
        Logger = None 
        Messaging = None 
    }
    
type ServerOptions = {
    Logger : ILogger option 
    Messaging : MessagingOptions 
}    
with 
    static member Make( logger, messaging ) = 
        { Logger = logger; Messaging = messaging }