# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

A C# console application that experiments with genetic algorithms. The main problem being solved is warehouse slotting optimization: placing products on shelves so that products appearing together on pick tickets end up close to each other and near the origin.

## Commands

Requires the .NET SDK (project targets `netcoreapp3.1`).

```bash
dotnet build TaskRunner.sln     # build
dotnet run --project TaskRunner # run the simulation
```

There are no tests or linters configured.

## Architecture

The genetic algorithm engine is decoupled from the problems it solves via the `IOrganism` interface (`TaskRunner/GeneticStructures/IOrganism.cs`), which defines `Score()`, `Mate()`, `Mutate()`, and `Clone()`. Lower scores are better — the runner minimizes.

- **`Runner` / `Population`** (`GeneticStructures/IOrganism.cs`): the generation loop. Each iteration it sorts organisms by score, clones the top `keepTopCutoff` survivors into the next generation, and fills the rest by mating random pairs from the top `matePopulationCutoff` organisms, then mutating. It uses two pre-allocated populations (`initialPopulation` and `emptyPopulation`) and swaps between them each generation to avoid allocations — organism methods like `Mate` and `Clone` write *into* the receiver rather than returning new objects.
- **`RandomGenerator`** (`GeneticStructures/RandomGenerator.cs`): a singleton wrapper around `System.Random`. The first call to `GetInstance(seed)` fixes the seed for the entire process (Program.cs seeds with 123 for reproducible runs); later calls ignore the seed argument.
- **Organisms** — problem implementations of `IOrganism`:
  - `Warehouse` (`Organisms/Warehouse.cs`): the main problem. Genome is `Shelves` (shelf index → nullable product ID), with a `ProductLocation` reverse lookup kept in sync via `SetShelf` — always mutate shelves through `SetShelf`, never directly. Scoring sums pairwise distances between products on the same pick ticket (using a precomputed `distanceLookup` table built in Program.cs) plus each ticket's center-of-mass distance from the origin. `Mate` is a single-point crossover that handles the permutation constraint: duplicate products are resolved by coin flip and products missing from the child are placed on random empty shelves.
  - `IncrementingBoard` (`Organisms/IncrementingBoard.cs`): a simpler example organism (evolve an array toward `arr[i] == i`), not currently wired into Program.cs.
- **`Program.cs`**: builds the scenario (100 products, 100 random pick tickets, a 30×10 shelf grid, the distance lookup table) and starts the `Runner`.

Note: namespaces don't match folder names everywhere (e.g. `IncrementingBoard` is in namespace `TaskRunner.Populations` but lives in `Organisms/`; `Population`, `Runner`, and `IOrganism` all live in `IOrganism.cs`). Follow the existing layout when editing.
