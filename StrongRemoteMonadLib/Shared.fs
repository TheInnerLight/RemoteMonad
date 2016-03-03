namespace StrongRemoteMonad

open System.IO
open System.Runtime.Serialization

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
