@tool
extends Node3D

@export var labyrinth_path: String = "res://labyrinth_generated.json"

@export var absorbing_material: Material
@export var reflecting_material: Material
@export var transparent_material: Material
@export var metallic_material: Material

const WALL_SIZE_X = 1.0
const WALL_SIZE_Z = 1.0
const WALL_SIZE_Y = 3.0

@export_tool_button("Load labyrinth walls!") var action = load_labyrinth

@export var generate_labyrinth_size_x: int = 13
@export var generate_labyrinth_size_z: int = 13

@export_tool_button("Regenerate labyrinth walls JSON file!") var action2 = generate_labyrinth

func add_as_child_to_scene(new_node: Node3D, parent_node: Node3D):
	parent_node.add_child(new_node)
	# The line below is required to make the node visible in the Scene tree dock
	# and persist changes made by the tool script to the saved scene file.
	new_node.owner = get_tree().edited_scene_root
	

func load_labyrinth():
	if not Engine.is_editor_hint():
		return

	# Remove existing children (clean previous generation)
	for child in get_children():
		remove_child(child)
		child.queue_free()

	var file = FileAccess.open(labyrinth_path, FileAccess.READ)
	if not file:
		push_error("Failed to open labyrinth.json")
		return

	var json_text = file.get_as_text()
	var data = JSON.parse_string(json_text)
	if data == null:
		push_error("Failed to parse JSON")
		return

	if not data.has("walls"):
		push_error("No walls in JSON")
		return

	for wall_data in data["walls"]:
		create_wall_segment(wall_data)
		

func create_wall_segment(wall_data):
	var material_type = wall_data["material"]
	var wall = StaticBody3D.new()
	wall.position = get_position_from_json(wall_data)
	wall.name = "Wall-"+str(wall.position)
	
	var mesh_instance = MeshInstance3D.new()
	var mesh = BoxMesh.new()
	mesh.size = get_size_from_json(wall_data)
	mesh_instance.mesh = mesh
	
	var material = get_material_by_type(material_type)
	if material:
		mesh_instance.material_override = material

	var collision = CollisionShape3D.new()
	var shape = BoxShape3D.new()
	shape.size = get_size_from_json(wall_data)
	collision.shape = shape
	
	add_as_child_to_scene(wall, self)
	add_as_child_to_scene(mesh_instance, wall)
	add_as_child_to_scene(collision, wall)
	

func get_position_from_json(wall_data) -> Vector3:
	if wall_data.has("position"):
		var position = wall_data["position"]
		return Vector3(position["x"], WALL_SIZE_Y/2, position["z"])
	else:
		var start_pos = Vector2(wall_data["startPosition"]["x"], wall_data["startPosition"]["z"])
		var end_pos = Vector2(wall_data["endPosition"]["x"], wall_data["endPosition"]["z"])
		var center = (start_pos + end_pos) * 0.5
		return Vector3(center.x, WALL_SIZE_Y/2, center.y)

func get_size_from_json(wall_data) -> Vector3:
	if wall_data.has("position"):
		return Vector3(WALL_SIZE_X, WALL_SIZE_Y, WALL_SIZE_Z)
	else:
		var start_pos = Vector2(wall_data["startPosition"]["x"], wall_data["startPosition"]["z"])
		var end_pos = Vector2(wall_data["endPosition"]["x"], wall_data["endPosition"]["z"])
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
		"metallic":
			return metallic_material
		_:
			return null


func generate_labyrinth():
	generate_labyrinth_from_size(generate_labyrinth_size_x, generate_labyrinth_size_z)


# Generate a labyrinth and save it as JSON
func generate_labyrinth_from_size(sizeX: int, sizeZ: int) -> void:
	# Ensure minimum size
	sizeX = max(sizeX, 3)
	sizeZ = max(sizeZ, 3)
	
	# Make sure dimensions are odd to work with the algorithm
	if sizeX % 2 == 0:
		sizeX += 1
	if sizeZ % 2 == 0:
		sizeZ += 1
	
	var walls = generate_labyrinth_walls(sizeX, sizeZ)
	var labyrinth_data = {
		"size": {"x": sizeX, "z": sizeZ},
		"walls": walls
	}
	
	# Convert to JSON and save
	var json_string = JSON.stringify(labyrinth_data, "\t")
	var file = FileAccess.open(labyrinth_path, FileAccess.WRITE)
	if file:
		file.store_string(json_string)
		file.close()
		print("Labyrinth saved to: ", labyrinth_path)
	else:
		print("Error: Could not save labyrinth file")


# Generate the wall data for the labyrinth
func generate_labyrinth_walls(sizeX: int, sizeZ: int) -> Array:
	var walls = []
	
	# Create a grid to track which cells are walls (true) or paths (false)
	# Only create interior grid, outer walls will be handled separately
	var grid = []
	for x in range(sizeX):
		grid.append([])
		for z in range(sizeZ):
			# Start with everything as walls, we'll carve out paths
			grid[x].append(true)
	
	# Generate the maze using recursive backtracking
	generate_maze_recursive(grid, sizeX, sizeZ)
	
	# Convert interior grid to wall positions (skip outer boundary)
	for x in range(0, sizeX):
		for z in range(0, sizeZ):
			if grid[x][z]:  # If this cell should be a wall
				walls.append({
					"position": {"x": x, "z": z},
					"material": "reflecting"
				})
	
	return walls

# Recursive backtracking maze generation
func generate_maze_recursive(grid: Array, sizeX: int, sizeZ: int) -> void:
	var stack = []
	var visited = []
	
	# Initialize visited array
	for x in range(sizeX):
		visited.append([])
		for z in range(sizeZ):
			visited[x].append(false)
	
	# Start from position (1,1) - first valid path cell
	var current_x = 1
	var current_z = 1
	grid[current_x][current_z] = false  # Make it a path
	visited[current_x][current_z] = true
	
	# Also ensure entry path is accessible
	if current_z == 1:
		grid[0][1] = false  # Clear path to potential entry
	
	while true:
		var neighbors = get_unvisited_neighbors(current_x, current_z, visited, sizeX, sizeZ)
		
		if neighbors.size() > 0:
			# Choose a random neighbor
			var next = neighbors[randi() % neighbors.size()]
			stack.push_back({"x": current_x, "z": current_z})
			
			# Remove wall between current and next
			var wall_x = current_x + (next.x - current_x) / 2
			var wall_z = current_z + (next.z - current_z) / 2
			grid[wall_x][wall_z] = false
			grid[next.x][next.z] = false
			
			visited[next.x][next.z] = true
			current_x = next.x
			current_z = next.z
			
			# If we're near the exit, ensure connectivity
			if next.x == sizeX - 2 and next.z == sizeZ - 2:
				grid[sizeX - 1][sizeZ - 2] = false  # Clear path to potential exit
		elif stack.size() > 0:
			# Backtrack
			var prev = stack.pop_back()
			current_x = prev.x
			current_z = prev.z
		else:
			break

# Get unvisited neighbors that are 2 cells away (to maintain wall thickness)
func get_unvisited_neighbors(x: int, z: int, visited: Array, sizeX: int, sizeZ: int) -> Array:
	var neighbors = []
	var directions = [
		{"x": 0, "z": 2},   # North
		{"x": 2, "z": 0},   # East
		{"x": 0, "z": -2},  # South
		{"x": -2, "z": 0}   # West
	]
	
	for dir in directions:
		var new_x = x + dir.x
		var new_z = z + dir.z
		
		if new_x >= 1 and new_x < sizeX - 1 and new_z >= 1 and new_z < sizeZ - 1:
			if not visited[new_x][new_z]:
				neighbors.append({"x": new_x, "z": new_z})
	
	return neighbors
