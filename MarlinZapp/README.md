## Prerequisites

Install the Godot Engine. Open the project by double clicking the file `project.godot`.

## Play

Run the project by clicking the play button in Godot.

## Generate a new labyrinth

Click on the Root node. In the inspector you can now see the script `generate_labyrinth.gd`. Adjust the parameters as you wish. Click on the generate button.

## Load a generated labyrinth

Click on the LabyrinthWalls node. In the inspector you can now see the script `load_labyrinth.gd`. Change the labyrinth path to your generated labyrinth json file. Click on the button for loading the labyrinth.

## Labyrinth representation

The labyrinth is represented as a JSON file. A small example for a valid labyrinth JSON is seen below. Walls are defined inside the `walls` property by giving a `position` or a `startPosition` and a `endPosition`. Please provide `x` and `z` property for the positions. Walls will always be 1 unit thick and 3 units high. The white material is called `reflecting`. You can also use `absorbing` for a black material, `transparent` for a glass-like material and `metallic` for a rough mirror-like material.

Optionally you can add the `lanterns` property where each lantern is defined by a `position` just like a wall and by a `direction` because the lanterns are wall lanterns and need a wall next to it so that they don't look weird.
```json
{
	"lanterns": [
		{
			"direction": "north",
			"position": {
				"x": 0,
				"z": 1
			}
		},
		{
			"direction": "south",
			"position": {
				"x": 10,
				"z": 1
			}
		}
	],
	"walls": [
		{
			"startPosition": {
				"x": 0,
				"z": 0
			},
			"endPosition": {
				"x": 10,
				"z": 0
			},
			"material": "reflecting"
		},
		{
			"position": {
				"x": 0,
				"z": 2
			},
			"material": "reflecting"
		}
	]
}
```