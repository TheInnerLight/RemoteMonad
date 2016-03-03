namespace StrongRemoteMonad

open System.IO
open System.Runtime.Serialization

open Serialisation
open Shared

/// A queue of commands
type Queue = Command list

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