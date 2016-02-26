﻿namespace StrongRemoteMonad

open System.Reflection
open FSharp.Reflection
open System.IO
open System.Runtime.Serialization
open System.Runtime.Serialization.Json

/// Serialisation Functions
module Serialisation =
    /// Serialise some object of type <'a> to a JSON string
    let serialise<'a> (data :'a) = 
        let jsonSerializer = new DataContractJsonSerializer(typedefof<'a>)
        use stream = new MemoryStream()
        jsonSerializer.WriteObject(stream, data)
        System.Text.Encoding.ASCII.GetString <| stream.ToArray()
    /// Deserialise some object of type <'a> from a JSON string
    let deserialise<'a> (str : string) =
        let jsonSerializer = new DataContractJsonSerializer(typedefof<'a>)
        use stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes str)
        jsonSerializer.ReadObject(stream) :?> 'a
    /// Helper function for generating Known Types for Union types
    let unionKnownTypeHelper<'a>() =
        typeof<'a>.GetNestedTypes(BindingFlags.Public ||| BindingFlags.NonPublic)
        |> Array.filter FSharpType.IsUnion

open Serialisation

/// Types shared between local and remote services
module Shared =
    /// A command which represents some activity performed remotely for which no response is generated
    [<KnownType("KnownTypes")>]
    type Command =
        |Say of string
        /// Known types associated with serialisation of this Union
        static member KnownTypes() = unionKnownTypeHelper<Command>()
        
    /// A Serialisable Procedure, similar to the Procedure but we no longer contain the return type information
    [<KnownType("KnownTypes")>]
    type SerialisableProcedure =
        |Temperature
        |Toast of int
        /// Known types associated with serialisation of this Union
        static member KnownTypes() = unionKnownTypeHelper<SerialisableProcedure>()

open Shared

/// Local service type definitions
module Local =
    /// Strongly typed procedure which returns a result of type 'a
    type Procedure<'a> = 
        |Temperature
        |Toast of int
        // this type cannot be serialised because we cannot deal with the generic type parameter 'a

    /// Read the reply from a procedure using the type information from the procedure
    let readProcedureReply (proc : Procedure<'a>) str =
        deserialise<'a> str

/// Remote service type definitions
module Remote =
    /// A packet containing only commands
    [<KnownType("KnownTypes")>]
    type AsyncPacket =
        |AsyncPacket of Command array
        /// Known types associated with serialisation of this Union
        static member KnownTypes() = unionKnownTypeHelper<AsyncPacket>()  

    /// A packet containing commands and a procedure call
    [<KnownType("KnownTypes")>]
    type SyncPacket =
        |SyncPacket of Command array * SerialisableProcedure
        /// Known types associated with serialisation of this Union
        static member KnownTypes() = unionKnownTypeHelper<SyncPacket>()  

    /// A device with two communication channels
    type Device = 
        |Device of (string -> string) * (string -> unit)

    /// Returns the synchronous communication channel for the device
    let sync = function
        |Device (f, _) -> f

    /// Returns the asychronous communication channel for the device
    let async = function
        |Device (_, f) -> f

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

    /// A device that simulates communication
    let device =
        Device (execSyncPacket << deserialise, execAsyncPacket << deserialise)

    /// Convert a Procedure to an RProcedure for serialisation
    let procedureToRProcedure = function
        |Local.Temperature -> Temperature
        |Local.Toast time -> Toast time

