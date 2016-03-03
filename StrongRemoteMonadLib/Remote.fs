namespace StrongRemoteMonad

open System.IO
open System.Runtime.Serialization

open Serialisation
open Shared

/// Remote service type definitions
module Remote =
    /// A device with two communication channels
    type Device = 
        |Device of (string -> string) * (string -> unit)

    /// Execute an RCommand
    let execRCommand = function
        |Say str -> printfn "Remote: %s" str

    /// Execute an RProcedure
    let execRProcedure = function
        |Temperature ->
            let rand = System.Random()
            serialise (rand.Next(50,100))
        |Toast time ->
            printfn "Remote: Toasting..."
            Async.RunSynchronously << Async.Sleep <| 1000*time // sleep for specified time in seconds
            printfn "Remote: Done!"
            serialise ()

    /// Execute a Synchronous packet (one which contains commands and a procedure)
    let execSyncPacket = function
        |SyncPacket (commands, procedure) ->
            Array.iter (execRCommand) commands
            execRProcedure procedure
    
    /// Execute a Asynchronous packet (one which contains just commands)
    let execAsyncPacket = function
        |AsyncPacket (commands) ->
            Array.iter (execRCommand) commands