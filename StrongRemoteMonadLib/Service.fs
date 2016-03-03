namespace StrongRemoteMonad

open Remote
open Shared
open Serialisation

/// The local view of the remote service
module Service =
    /// Convert a Procedure to an RProcedure for serialisation
    let private procedureToRProcedure = function
        |Local.Temperature -> Temperature
        |Local.Toast time -> Toast time

    /// Reverse a queue and covert to a command array
    let private queueToArray : Queue -> Command[] =
        Array.ofList << List.rev

    /// A device that simulates communication
    let device =
        Device (execSyncPacket << deserialise, execAsyncPacket << deserialise)

    /// Returns the synchronous communication channel for the device
    let sync = function
        |Device (f, _) -> f

    /// Returns the asychronous communication channel for the device
    let async = function
        |Device (_, f) -> f

    /// Creates an async packet from a command queue
    let createAsyncPacket queue =
        AsyncPacket <| queueToArray queue

    /// Creates a sync packet from a command queue and procedure
    let createSyncPacket queue proc  =
        SyncPacket(queueToArray queue, procedureToRProcedure proc)