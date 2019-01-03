namespace Example.Rpc

module Helpers = 

    let WrapMethod (fn:IRpcRequestContext*'ReqT->'RespT) =
        fun (context,req) ->
            try 
                let result = fn (context,req)
                Example.Rpc.Result.Make( Success.Make( result ) )
            with 
            | _ as ex ->
                Example.Rpc.Result.Make( Error.Make( ex.Message, false ) )
