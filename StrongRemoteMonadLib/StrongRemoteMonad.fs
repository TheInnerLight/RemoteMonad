namespace StrongRemoteMonad



/// The remote type we use to build the state/reader monad 
type Remote<'a> = Remote of (Remote.Device -> Queue -> 'a * Queue)

/// Functions on the strong remote monad
[<AutoOpen>]
module StrongRemoteMonad =
    /// Unwraps the Remote data type
    let runRemote = function
        |Remote g -> g
    /// Monadic return for Remote
    let return' a = Remote(fun device queue -> a, queue)
    /// Monadic bind for Remote
    let bind (ra : Remote<'a>) (f : 'a -> Remote<'b>) =
        Remote(fun device queue ->
            let (a, queue') = runRemote ra device queue
            runRemote (f a) device queue')
    /// Provides a way of accessing Remote's device
    let ask = Remote (fun device queue -> device, queue)
    /// Provides a way of accessing Remote's command queue
    let get = Remote (fun _ queue -> queue, queue)
    /// Ignores any existing queue and places a new one into the Remote
    let put queue = Remote (fun _ _ -> (), queue)
    /// Allows a Remote's command queue to be transformed by some supplied function
    let modify f = Remote (fun _ queue -> (), f queue)

    /// A Builder for computation expressions pertaining to the strong remote monad
    type RemoteBuilder() =
        /// Monadic return for Remote
        member this.Return a = return' a
        /// Bare return for Remote
        member this.ReturnFrom a : Remote<'a> = a
        /// Monadic bind for Remote
        member this.Bind (a, f) = bind a f
        /// The zero case for Remote
        member this.Zero () = return' ()

    /// The remote computation expression definition
    let remote = RemoteBuilder()

    /// Send accumulated command queues to the device
    let send device ra =
        let (a, queue) = runRemote ra device []
        match queue with
        |[] -> ()
        |_ ->
            let asyncPacket = Remote.createAsyncPacket queue
            Remote.async device (Serialisation.serialise asyncPacket)
        a

    /// Send a command to the remote device
    let sendCommand cmd =
        modify (fun queue -> cmd :: queue)

    /// Send a procedure to the remote device
    let sendProcedure proc =
        remote{
            let! dev = ask
            let! queue = get
            let syncPacket = Remote.createSyncPacket queue proc
            let str = Remote.sync dev (Serialisation.serialise syncPacket)
            do! put []
            return (Local.readProcedureReply proc str)
        }

    /// A command which instructs the device to "say" something
    let say str = sendCommand (Shared.Say str)

    /// A procedure which requests the current temperature of the toast
    let temperature<'a> : Remote<'a> = sendProcedure (Local.Temperature)

    /// A procedure which toasts the toast for a supplied number of seconds
    let toast seconds = sendProcedure (Local.Toast seconds)



