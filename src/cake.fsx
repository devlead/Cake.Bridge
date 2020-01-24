//////////////////////////////////////////////////////////////////////
// DEPENDENCIES
//////////////////////////////////////////////////////////////////////
#r "../../Cake.Core.0.36.0/lib/net46/Cake.Core.dll"
#r "../../Cake.Common.0.36.0/lib/net46/Cake.Common.dll"
#r "../lib/net46/Cake.Bridge.dll"

namespace CakeAdapter

//////////////////////////////////////////////////////////////////////
// NAMESPACE IMPORTS
//////////////////////////////////////////////////////////////////////
open Cake.Common
open Cake.Common.Diagnostics
open Cake.Common.IO
open Cake.Common.Tools.DotNetCore
open Cake.Common.Tools.DotNetCore.Build
open Cake.Common.Tools.DotNetCore.Pack
open Cake.Core
open Cake.Core.IO
open System
//open CakeBridge

module CakeModule =
// "Magic" to handle implicit conversion (which F# isn't really supporting)
  let inline (!>) (x:^a) : ^b = ((^a or ^b) : (static member op_Implicit : ^a -> ^b) x)
  let context                 = CakeBridge.Context

  type CTB<'T when 'T :> CakeTask> = CakeTaskBuilder<'T>

  let setup     (f : ICakeContext -> unit)              : unit        = CakeBridge.Setup      (Action<_> f)
  let tearDown  (f : ICakeContext -> unit)              : unit        = CakeBridge.Teardown   (Action<_> f)
  let task      (name : string)                         : CTB<_>      = CakeBridge.Task       name
  let runTarget (target : string)                       : CakeReport  = CakeBridge.RunTarget  target

  let does          (f : unit -> unit) (task : CTB<_>)  : CTB<_>      = task.Does(f)
  let isDependentOn (other : CTB<_>)   (task : CTB<_>)  : CTB<_>      = task.IsDependentOn other
