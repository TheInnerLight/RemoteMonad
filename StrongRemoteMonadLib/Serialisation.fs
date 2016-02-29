namespace StrongRemoteMonad

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