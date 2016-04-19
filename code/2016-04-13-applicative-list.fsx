(**
\---
layout: post
title: "Applicative: Lists"
tags: [applicative,list]
description: Building an Applicative Functor for the list type
keywords: f#, fsharp, list, applicative, functor, functional, programming
\---
*)

(*** hide ***)
module Main

(**
In [Understanding apply]({% post_url 2016-03-31-applicative-functors %}) I primarily
used the `Option` type to show how you implement and use an *Applicative Functor*.
But the concept also works for any other type. This time I want to show
you the idea of an *Applicative* with a list, what it means, what you can do
with it and how `apply` works.

## Implementing `apply`

Currently the `List` type don't offer a `apply` function. So we must write it on our own.
As we learned in [Understanding bind]({% post_url 2016-04-03-understanding-bind %}) we
could implement `apply` with `bind`. Because `List.collect` is the `bind` function (you
can see that by inspecting the function-signature), we could implement `apply` like this.

    let apply lf lx =
        lf |> List.collect (fun f ->
        lx |> List.collect (fun x ->
            [f x]
        ))

Although it is good to know this, this time we implement `apply` from scratch. So we can
better understand how `apply` works.

The general idea of `apply` is easy. We need to implement a function that expects a
function as it's first argument, and a value as the second argument. But both values
are *boxed* in our type. The only thing we must do is somehow call our function with
our value. So we need a function that can handle the following function signature:

    list<('a -> 'b)> -> list<'a> -> list<'b>

If it is unclear why we get a `list<'b>` as a result. It always makes sense to remember
what `apply` does as a single argument function. It just takes a `A<('a -> 'b)>` and
transform it into a new function `A<'a> -> A<'b>`. Here `A` stands for any *Applicative*
type.

For a list it has the following meaning:

1. We get a list of functions as the first value
1. We get a list of values as the second argument
1. *Unboxing* a list means we just loop over the list
1. Then we just execute every function with every value

We can implement `apply` like this:
*)

let apply lf lx = [
    for f in lf do
    for x in lx do
        yield f x
]

let (<*>) = apply 

(**
## Working with `apply`

We keep it easy, so we just create two to four arguments functions that just adds it's
input together.
*)

let add2 x y     = x + y
let add3 x y z   = x + y + z
let add4 x y z w = x + y + z + w

(**
Usually we need a `return` function, but we can easily lift any values into a list by just
surrounding it with `[]`, so we will skip this one. The idea of `apply` means every argument
of a function can now be a boxed type. That means, instead of just passing two `int`
to `add2` we can now pass a `list<int>` as the first argument and a `list<int>`
as the second argument, and so on. We now can write something like this.
*)

[add2] <*> [1;2;3] <*> [10;20]
[add3] <*> [1;2;3] <*> [10;20] <*> [5]
[add4] <*> [1;2;3] <*> [10;20] <*> [5] <*> [100;200]

(**
Let's see what those function calls produces

    [11; 21; 12; 22; 13; 23]
    [16; 26; 17; 27; 18; 28]
    [116; 216; 126; 226; 117; 217; 127; 227; 118; 218; 128; 228]

What we get back is the result of every input combination. Our `add2` call expands to

    [
        add2 1 10
        add2 1 20
        add2 2 10
        add2 2 20
        add2 3 10
        add2 3 20
    ]

## How `apply` works

At this point it is interesting to see how `apply` actually works to get a better understanding
why we get those results. At first we should remember how the operator `<*>` works. Our 
apply operator is just a infix function. It uses the the thing on the left-side as the
first argument, and the thing on the right-side as it's second argument. Instead of

    [lang=fsharp]
    [f] <*> [1;2;3]

we also could write 
    
    apply [f] [1;2;3]

When we have a term like `[add2] <*> [1;2;3] <*> [10;20]` it means, first `[add2] <*> [1;2;3]`
is executed and it will return a result! This is exactly how a normal function call work.
Even a normal function call like `add2 1 10` basically works by first executing `add2 1`
returning a new function and then pass `10` to it. That's why we also can write. `(add2 1) 10`
and it produces the same result. With `apply` or `<*>` it is the same, our term is
basically interpreted as

    ( [add2] <*> [1;2;3] )     <*> [10;20]

So what happens first is, that we call `apply` and pass it `[add2]` as it's first-argument
and `[1;2;3]` as it's second argument. Remember that the only thing we do is just loop
through our list of functions, and loop through our list of values and just call our function
with the value. So after `[add2] <*> [1;2;3]` we get a new list back, containing.

    [
        add2 1
        add2 2
        add2 3
    ]

Add this point it probably becomes more clear why we can view `apply` as some kind of
*Partial Application* for *boxed* functions. The only thing that `apply` does is take
a *boxed* function and a *boxed* value and execute it. But it only does it for the
next argument. The first `apply` call returns a new list with three *Partial Applied*
functions. This is then passed to the next `apply` function. We now get:

    [add2 1; add2 2; add2 3] <*> [10;20]

We end up again with a `apply` call that this time loops through three functions
and two values. It produces the following function calls.

    [
        add2 1 10
        add2 1 20
        add2 2 10
        add2 2 20
        add2 3 10
        add2 3 20
    ]

And this will result in

    [11; 21; 12; 22; 13; 23]

To get a hang of it, let's once again go through the `add4` example and visualize
every step. We start with

    [add4] <*> [1;2;3] <*> [10;20] <*> [5] <*> [100;200]

The first `apply` call then produces

    [
        add4 1
        add4 2
        add4 3
    ]

We then use this result with `apply` and use `[10;20]` next.

    [
        add4 1 10
        add4 1 20
        add4 2 10
        add4 2 20
        add4 3 10
        add4 3 20
    ]

Then we use this list of functions with `[5]` and we get

    [
        add4 1 10 5
        add4 1 20 5
        add4 2 10 5
        add4 2 20 5
        add4 3 10 5
        add4 3 20 5
    ]

Finally we use `[100;200]` on this list, and we get.

    [
        add4 1 10 5 100
        add4 1 10 5 200
        add4 1 20 5 100
        add4 1 20 5 200
        add4 2 10 5 100
        add4 2 10 5 200
        add4 2 20 5 100
        add4 2 20 5 200
        add4 3 10 5 100
        add4 3 10 5 200
        add4 3 20 5 100
        add4 3 20 5 200
    ]

The last call executes the functions, so we get the result.

    [116; 216; 126; 226; 117; 217; 127; 227; 118; 218; 128; 228]

## Using `apply`

In general what we can do with an *Applicative* for a list is call a function and upgrade
any argument to a list. So we get all possible results. With this idea we also can
easily generate [Cartesian Products](https://en.wikipedia.org/wiki/Cartesian_product).

A *Cartesian Product* is just the result of all possible combinations. In this way
we also can generate a set of Cards of a Card Game.
*)

type Suit = 
    | Clubs | Diamonds | Hearts | Spades

type Rank = 
    | Ace | Two | Three | Four | Five | Six | Seven | Eight | Nine | Ten
    | Jack | Queen | King

type Card = Card of Suit * Rank

(**
We now can generate all possible Cards by using.
*)

let suits = [Clubs;Diamonds;Hearts;Spades]
let ranks = [Ace;Two;Three;Four;Five;Six;Seven;Eight;Nine;Ten;Jack;Queen;King]

let cards = [fun s r -> Card (s,r)] <*> suits <*> ranks

(** We now get a list of all 52 cards as a result. *)

let cards = [
    Card (Clubs,Ace); Card (Clubs,Two); Card (Clubs,Three); Card (Clubs,Four);
    Card (Clubs,Five); Card (Clubs,Six); Card (Clubs,Seven); Card (Clubs,Eight);
    Card (Clubs,Nine); Card (Clubs,Ten); Card (Clubs,Jack); Card (Clubs,Queen);
    Card (Clubs,King); Card (Diamonds,Ace); Card (Diamonds,Two);
    Card (Diamonds,Three); Card (Diamonds,Four); Card (Diamonds,Five);
    Card (Diamonds,Six); Card (Diamonds,Seven); Card (Diamonds,Eight);
    Card (Diamonds,Nine); Card (Diamonds,Ten); Card (Diamonds,Jack);
    Card (Diamonds,Queen); Card (Diamonds,King); Card (Hearts,Ace);
    Card (Hearts,Two); Card (Hearts,Three); Card (Hearts,Four);
    Card (Hearts,Five); Card (Hearts,Six); Card (Hearts,Seven);
    Card (Hearts,Eight); Card (Hearts,Nine); Card (Hearts,Ten);
    Card (Hearts,Jack); Card (Hearts,Queen); Card (Hearts,King);
    Card (Spades,Ace); Card (Spades,Two); Card (Spades,Three);
    Card (Spades,Four); Card (Spades,Five); Card (Spades,Six);
    Card (Spades,Seven); Card (Spades,Eight); Card (Spades,Nine);
    Card (Spades,Ten); Card (Spades,Jack); Card (Spades,Queen);
    Card (Spades,King)
]

(**
The *Cartesian Product* is also the idea how we view relational data. We could for example
create two lists that refers to each other, with `apply` we then can easily create the
*Cartesian Product* and filter those data.
*)

type Person = {
    Id:   int
    Name: string
} with
    static member create id name = {Id=id; Name=name}

type Like = {
    PersonId: int
    Name:     string
} with
    static member create pid name = {PersonId=pid; Name=name}

let persons = [
    Person.create 1 "David"
    Person.create 2 "Markus"
    Person.create 3 "Björn"
]

let likes = [
    Like.create 1 "Pizza"
    Like.create 2 "Pizza"
    Like.create 3 "Pizza"
    Like.create 3 "Coffee"
    Like.create 1 "Tea"
    Like.create 2 "Tea"
]

(**
We now can create the *Cartesian Product* of those Data. And afterwards filter it.
*)

let likesTea = 
    [fun p l -> p,l] <*> persons <*> likes
    |> List.filter (fun (person,like) -> person.Id = like.PersonId)
    |> List.filter (fun (person,like) -> like.Name = "Tea")
    |> List.map    (fun (person,like) -> person.Name)

(**
This will return: ` ["David"; "Markus"]`

and resembles a SQL-Statement like:

    [lang=sql]
    SELECT p.Name
    FROM   person p, likes l
    WHERE  p.Id = l.PersonId
    AND    l.Name = "Tea"

Sure, most stuff is basically *List-Processing* at this point, but `apply` is just another
functions in a tool-set that opens up some possibilities. And at this point you probably
even see the connection between functional list processing and SQL, or in general the
C# LINQ Feature.
*)  