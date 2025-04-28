extends Node3D

# Preload materials (you create these in the editor and assign paths)
@export var absorbing_material: Material
@export var reflecting_material: Material
@export var transparent_material: Material

# Path to the labyrinth JSON
@export var labyrinth_path: String = "res://labyrinth.json"

# Size constants
const WALL_SIZE_X = 1.0
const WALL_SIZE_Z = 1.0
const WALL_SIZE_Y = 3.0

func _ready():
	load_labyrinth()

func load_labyrinth():
	var file = FileAccess.open(labyrinth_path, FileAccess.READ)
	if not file:
		print("Failed to open labyrinth.json")
		return

	var json_text = file.get_as_text()
	var data = JSON.parse_string(json_text)
	if data == null:
		print("Failed to parse JSON")
		return

	if not data.has("walls"):
		print("No walls in JSON")
		return

	for wall_data in data["walls"]:
		create_wall_segment(wall_data)

func create_wall_segment(wall_data):
	var material_type = wall_data["material"]

	# Create the wall node
	var wall = StaticBody3D.new()
	wall.position = get_position_from_json(wall_data)
	
	# Create the visual part (mesh)
	var mesh_instance = MeshInstance3D.new()
	var mesh = BoxMesh.new()
	mesh.size = get_size_from_json(wall_data)
	mesh_instance.mesh = mesh
	
	var material = get_material_by_type(material_type)
	if material:
		mesh_instance.material_override = material
	
	wall.add_child(mesh_instance)

	# Create the collision part
	var collision = CollisionShape3D.new()
	var shape = BoxShape3D.new()
	shape.size = get_size_from_json(wall_data)
	collision.shape = shape

	wall.add_child(collision)

	# Add the wall to the scene
	add_child(wall)


func get_position_from_json(wall_data) -> Vector3:
	if wall_data.has("position"):
		var position = wall_data["position"]
		return Vector3(position["x"], WALL_SIZE_Y/2, position["y"])
	else:
		assert(wall_data.has("startPosition") and wall_data.has("endPosition"), "Walls need a position or startPosition and endPosition!")
		var start_pos : Vector2 = Vector2(wall_data["startPosition"]["x"], wall_data["startPosition"]["y"])
		var end_pos : Vector2 = Vector2(wall_data["endPosition"]["x"], wall_data["endPosition"]["y"])
		var center = (start_pos + end_pos) * 0.5
		return Vector3(center.x, WALL_SIZE_Y/2, center.y)


func get_size_from_json(wall_data) -> Vector3:
	if wall_data.has("position"):
		return Vector3(WALL_SIZE_X,WALL_SIZE_Y,WALL_SIZE_Z)
	else:
		assert(wall_data.has("startPosition") and wall_data.has("endPosition"), "Walls need a position or startPosition and endPosition!")
		var start_pos : Vector2 = Vector2(wall_data["startPosition"]["x"], wall_data["startPosition"]["y"])
		var end_pos : Vector2 = Vector2(wall_data["endPosition"]["x"], wall_data["endPosition"]["y"])
		assert(start_pos.x == end_pos.x or start_pos.y == end_pos.y, "Start and end positions need the same x or the same y coordinates.")
		if start_pos.x == end_pos.x:
			return Vector3(WALL_SIZE_X, WALL_SIZE_Y, WALL_SIZE_Z * start_pos.distance_to(end_pos) + 1)
		else:
			return Vector3(WALL_SIZE_X * start_pos.distance_to(end_pos) + 1, WALL_SIZE_Y, WALL_SIZE_Z)
		

func get_material_by_type(material_type: String) -> Material:
	match material_type:
		"absorbing":
			return absorbing_material
		"reflecting":
			return reflecting_material
		"transparent":
			return transparent_material
		_:
			return null
