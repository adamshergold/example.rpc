namespace Example.Rpc

open Example.Serialisation

type IRpcRequestContext =
    inherit ITypeSerialisable 
    abstract RequestId : string with get 
    abstract Tag : string option
    abstract UserId : string with get 
    
type IRpcError = 
    inherit ITypeSerialisable
    abstract Message : string with get 
    abstract Retryable : bool with get 

type IRpcInfo = 
    inherit ITypeSerialisable
    abstract ElapsedTimeMilliseconds : int with get 

type IRpcContent =
    abstract IsWrapped : bool with get
    abstract Value : ITypeSerialisable with get
    abstract Wrapped : ITypeWrapper with get
                
type IRpcSuccess = 
    inherit ITypeSerialisable
    abstract Content : IRpcContent with get 
    abstract Info : IRpcInfo option with get     
    abstract As<'T when 'T :> ITypeSerialisable> : unit -> 'T

type IRpcResult =
    inherit ITypeSerialisable
    abstract IsSuccess : bool with get
    abstract Error : IRpcError with get 
    abstract Success : IRpcSuccess with get
    
type IRpcRegister = 
    abstract Register<'ReqT when 'ReqT :> ITypeSerialisable> : (IRpcRequestContext* 'ReqT -> IRpcResult) -> unit
        
type IRpcClient = 
    inherit System.IDisposable
    inherit IRpcRegister
    abstract Call<'ReqT when 'ReqT :> ITypeSerialisable > : IRpcRequestContext * 'ReqT -> Async<IRpcResult>

type IRpcServer = 
    inherit System.IDisposable
    inherit IRpcRegister

type ClientServerHandle = 
    | ClientHandle of IRpcClient
    | ServerHandle of IRpcServer
with 
    static member Make( client: IRpcClient ) = 
        ClientServerHandle.ClientHandle( client )
        
    static member Make( server: IRpcServer ) = 
        ClientServerHandle.ServerHandle( server )
        
    member this.Register = 
        match this with 
        | ClientHandle(client) -> client :> IRpcRegister
        | ServerHandle(server) -> server :> IRpcRegister 
                
    interface System.IDisposable
        with 
            member this.Dispose() = 
                match this with 
                | ClientHandle(client) -> client.Dispose()
                | ServerHandle(server) -> server.Dispose()
                        
        