module Spin

open System.Diagnostics
open Spinner

let private printNoExecutable () =
    eprintf "No executable was specified as argument.\n"

let private printHelp () =
    printf "Usage: spin [options] <executable> [<arguments to executable>]\n
    Options:
        -r
        --reverse        Reverse the direction of the spinner graphic.
        -h
        --help           Print this message and exit.\n"

type FlagParseResults(args : string array, setFlags: Set<char>, errorHappened: bool) =
    struct
        member this.remainingArgs = args
        member this.flags = setFlags
        member this.hasError = errorHappened

        member this.hasFlag(c : char) =
            this.flags.Contains(c)
    end

let private printWrongArgument (flags : FlagParseResults) =
    if flags.hasError then
        eprintf $"Could not accept option '%s{flags.remainingArgs[0]}'.\n"

(* Parse the flag arguments (i.e. arguments of the form -_ and --_).
Returns a triple with the remaining arguments, the flags parsed, and a
boolean indicating whether an error was thrown.
If a flag was encountered that was not among the acceptable options,
then it will not be removed from the list of remaining arguments.
Short-circuits, so that any error causes an immediate return. *)
let rec private _parseFlags (args : string array) (flags : Set<char>) =
    if (args.Length = 0) then
        FlagParseResults(args, flags, false)
    else match args[0] with
            | "-r"
            | "--reverse" ->
                _parseFlags args[1..] (Set.add 'r' flags)
            | "-h"
            | "--help" ->
                _parseFlags args[1..] (Set.add 'h' flags)
            | str when str[0] = '-' -> FlagParseResults(args, flags, true)
            | _ -> FlagParseResults(args, flags, false)

let private parseFlags (args : string array) =
    _parseFlags args (Set<char>([]))

(* Execute a shell command.
   Returns the process instance. *)
let public exec (args : string array) =
    if (args.Length = 0) then
        ()

    let startInfo = ProcessStartInfo()
    startInfo.FileName <- args[0]
    startInfo.UseShellExecute <- false
    for arg in args[1..] do
        startInfo.ArgumentList.Add(arg)
    let p = new Process()
    p.StartInfo <- startInfo
    p.Start() |> ignore
    p

[<EntryPoint>]
let main args =
    let flags = parseFlags args
    if flags.hasError then
        printWrongArgument flags
        printHelp ()
        1
    else if flags.hasFlag('h') then
        printHelp ()
        0
    else if flags.remainingArgs.Length = 0 then
        printNoExecutable ()
        printHelp ()
        1
    else
        let func = match flags.hasFlag('r') with
                   | true -> spinBackwards
                   | false -> spin

        // Can also do:
        //let thread, writer = createSpinner func
        //thread.Start()
        let writer = startSpinner func
        let execProcess = exec flags.remainingArgs
        while not execProcess.HasExited do
            ()
        writer.WriteAsync(()) |> ignore
        0