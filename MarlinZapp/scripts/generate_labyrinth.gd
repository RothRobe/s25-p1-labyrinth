@tool
extends Node3D

@export var labyrinth_path: String = "res://labyrinth_generated.json"

@export var generate_labyrinth_size_x: int = 13
@export var generate_labyrinth_size_z: int = 13

@export_tool_button("Regenerate labyrinth JSON file!") var action2 = generate_labyrinth

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
	var lanterns = generate_lantern_positions(sizeX, sizeZ, walls)
	
	# Group individual wall blocks into larger wall segments
	walls = group_wall_segments(walls)
	
	var labyrinth_data = {
		"size": {"x": sizeX, "z": sizeZ},
		"walls": walls,
		"lanterns": lanterns
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
				})
	
	return walls

# Generate lantern positions in the labyrinth
func generate_lantern_positions(sizeX: int, sizeZ: int, walls: Array) -> Array:
	var lanterns = []
	
	# Create a set of wall positions for quick lookup
	var wall_positions = {}
	for wall in walls:
		var pos = wall.position
		wall_positions[str(pos.x) + "," + str(pos.z)] = true
	
	# Find all path cells (cells that are not walls)
	var path_cells = []
	for x in range(sizeX):
		for z in range(sizeZ):
			var key = str(x) + "," + str(z)
			if not wall_positions.has(key):
				path_cells.append({"x": x, "z": z})
	
	# Strategy 1: Place lanterns at intersections (path cells with 3+ adjacent paths)
	for cell in path_cells:
		var adjacent_paths = count_adjacent_paths(cell.x, cell.z, wall_positions, sizeX, sizeZ)
		if adjacent_paths >= 3:  # Intersection
			var direction = find_best_wall_direction(cell.x, cell.z, wall_positions, sizeX, sizeZ)
			if direction != "":
				lanterns.append({
					"position": {"x": cell.x, "z": cell.z},
					"direction": direction,
					"type": "intersection"
				})
	
	# Strategy 2: Place lanterns at dead ends
	for cell in path_cells:
		var adjacent_paths = count_adjacent_paths(cell.x, cell.z, wall_positions, sizeX, sizeZ)
		if adjacent_paths == 1:  # Dead end
			var direction = find_best_wall_direction(cell.x, cell.z, wall_positions, sizeX, sizeZ)
			if direction != "":
				lanterns.append({
					"position": {"x": cell.x, "z": cell.z},
					"direction": direction,
					"type": "dead_end"
				})
	
	# Strategy 3: Place additional lanterns along long corridors
	add_corridor_lanterns(path_cells, wall_positions, sizeX, sizeZ, lanterns)
	
	# Strategy 4: Ensure minimum lighting coverage
	ensure_minimum_coverage(path_cells, lanterns, 6, wall_positions, sizeX, sizeZ)  # Lantern every 6 units max
	
	return lanterns

# Find the best wall direction for placing a wall lantern
func find_best_wall_direction(x: int, z: int, wall_positions: Dictionary, sizeX: int, sizeZ: int) -> String:
	var directions = [
		{"name": "north", "x": 0, "z": 1},
		{"name": "east", "x": 1, "z": 0},
		{"name": "south", "x": 0, "z": -1},
		{"name": "west", "x": -1, "z": 0}
	]
	
	# Find all adjacent walls
	var wall_directions = []
	for dir in directions:
		var wall_x = x + dir.x
		var wall_z = z + dir.z
		
		# Check if it's within bounds and is a wall
		if wall_x >= 0 and wall_x < sizeX and wall_z >= 0 and wall_z < sizeZ:
			var key = str(wall_x) + "," + str(wall_z)
			if wall_positions.has(key):
				wall_directions.append(dir.name)
	
	# Prioritize walls based on context
	# For dead ends, prefer the wall opposite to the entrance
	# For intersections, prefer the least "busy" wall
	if wall_directions.size() > 0:
		# For simplicity, return the first available wall
		# You could add more sophisticated logic here
		return wall_directions[0]
	
	return ""  # No adjacent wall found
	
func count_adjacent_paths(x: int, z: int, wall_positions: Dictionary, sizeX: int, sizeZ: int) -> int:
	var count = 0
	var directions = [
		{"x": 0, "z": 1},   # North
		{"x": 1, "z": 0},   # East
		{"x": 0, "z": -1},  # South
		{"x": -1, "z": 0}   # West
	]
	
	for dir in directions:
		var new_x = x + dir.x
		var new_z = z + dir.z
		
		# Check bounds
		if new_x >= 0 and new_x < sizeX and new_z >= 0 and new_z < sizeZ:
			var key = str(new_x) + "," + str(new_z)
			if not wall_positions.has(key):
				count += 1
	
	return count

# Add lanterns along long corridors
func add_corridor_lanterns(path_cells: Array, wall_positions: Dictionary, sizeX: int, sizeZ: int, lanterns: Array) -> void:
	var lantern_positions = {}
	
	# Build existing lantern position lookup
	for lantern in lanterns:
		var pos = lantern.position
		lantern_positions[str(pos.x) + "," + str(pos.z)] = true
	
	# Find corridor segments and place lanterns every few cells
	for cell in path_cells:
		var adjacent_paths = count_adjacent_paths(cell.x, cell.z, wall_positions, sizeX, sizeZ)
		
		# If it's a corridor (exactly 2 connections) and no nearby lantern
		if adjacent_paths == 2:
			var has_nearby_lantern = false
			
			# Check for lanterns within radius of 3
			for dx in range(-3, 4):
				for dz in range(-3, 4):
					var check_x = cell.x + dx
					var check_z = cell.z + dz
					var key = str(check_x) + "," + str(check_z)
					if lantern_positions.has(key):
						has_nearby_lantern = true
						break
				if has_nearby_lantern:
					break
			
			# Place lantern if no nearby ones exist
			if not has_nearby_lantern:
				var direction = find_best_wall_direction(cell.x, cell.z, wall_positions, sizeX, sizeZ)
				if direction != "":
					var key = str(cell.x) + "," + str(cell.z)
					lantern_positions[key] = true
					lanterns.append({
						"position": {"x": cell.x, "z": cell.z},
						"direction": direction,
						"type": "corridor"
					})

# Ensure minimum coverage by adding lanterns where needed
func ensure_minimum_coverage(path_cells: Array, lanterns: Array, max_distance: int, wall_positions: Dictionary, sizeX: int, sizeZ: int) -> void:
	var lantern_positions = []
	
	# Extract lantern positions
	for lantern in lanterns:
		lantern_positions.append(lantern.position)
	
	# Check each path cell for coverage
	for cell in path_cells:
		var has_coverage = false
		
		# Check if within max_distance of any lantern
		for lantern_pos in lantern_positions:
			var distance = abs(cell.x - lantern_pos.x) + abs(cell.z - lantern_pos.z)  # Manhattan distance
			if distance <= max_distance:
				has_coverage = true
				break
		
		# Add lantern if no coverage
		if not has_coverage:
			var direction = find_best_wall_direction(cell.x, cell.z, wall_positions, sizeX, sizeZ)
			if direction != "":
				lanterns.append({
					"position": {"x": cell.x, "z": cell.z},
					"direction": direction,
					"type": "coverage"
				})
				lantern_positions.append({"x": cell.x, "z": cell.z})

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

# Group individual wall blocks into larger wall segments and give them a material
func group_wall_segments(individual_walls: Array) -> Array:
	var wall_segments = []
	var used_positions = {}
	
	# Create a lookup for wall positions
	var wall_positions = {}
	for wall in individual_walls:
		var pos = wall.position
		var key = str(pos.x) + "," + str(pos.z)
		wall_positions[key] = wall
	
	# Process each wall block
	for wall in individual_walls:
		var pos = wall.position
		var key = str(pos.x) + "," + str(pos.z)
		
		# Skip if already used in a segment
		if used_positions.has(key):
			continue
		
		var material = "reflecting"
		var rand = randi_range(0,100)
		if rand < 20:
			material = "absorbing"
		elif rand < 40:
			material = "transparent"
		elif rand < 50:
			material = "metallic"
		
		# Try to create horizontal segment first
		var horizontal_segment = create_horizontal_segment(pos.x, pos.z, wall_positions, used_positions, material)
		if horizontal_segment.length > 1:
			wall_segments.append(horizontal_segment.segment)
			continue
		
		# Try to create vertical segment
		var vertical_segment = create_vertical_segment(pos.x, pos.z, wall_positions, used_positions, material)
		if vertical_segment.length > 1:
			wall_segments.append(vertical_segment.segment)
			continue
		
		# Single block wall (no adjacent walls in straight line)
		used_positions[key] = true
		wall.material = material
		wall_segments.append(wall)
	
	return wall_segments

# Create a horizontal wall segment starting from given position
func create_horizontal_segment(start_x: int, start_z: int, wall_positions: Dictionary, used_positions: Dictionary, material: String) -> Dictionary:
	var end_x = start_x
	
	# Find the rightmost connected wall in the same row
	while true:
		var next_key = str(end_x + 1) + "," + str(start_z)
		if wall_positions.has(next_key) and not used_positions.has(next_key):
			end_x += 1
		else:
			break
	
	# Also check leftward from start position
	var actual_start_x = start_x
	while true:
		var prev_key = str(actual_start_x - 1) + "," + str(start_z)
		if wall_positions.has(prev_key) and not used_positions.has(prev_key):
			actual_start_x -= 1
		else:
			break
	
	var length = end_x - actual_start_x + 1
	
	# Mark all positions in this segment as used
	if length > 1:
		for x in range(actual_start_x, end_x + 1):
			var key = str(x) + "," + str(start_z)
			used_positions[key] = true
	
	return {
		"length": length,
		"segment": {
			"material": material,
			"startPosition": {"x": actual_start_x, "z": start_z},
			"endPosition": {"x": end_x, "z": start_z}
		}
	}

# Create a vertical wall segment starting from given position
func create_vertical_segment(start_x: int, start_z: int, wall_positions: Dictionary, used_positions: Dictionary, material: String) -> Dictionary:
	var end_z = start_z
	
	# Find the topmost connected wall in the same column
	while true:
		var next_key = str(start_x) + "," + str(end_z + 1)
		if wall_positions.has(next_key) and not used_positions.has(next_key):
			end_z += 1
		else:
			break
	
	# Also check downward from start position
	var actual_start_z = start_z
	while true:
		var prev_key = str(start_x) + "," + str(actual_start_z - 1)
		if wall_positions.has(prev_key) and not used_positions.has(prev_key):
			actual_start_z -= 1
		else:
			break
	
	var length = end_z - actual_start_z + 1
	
	# Mark all positions in this segment as used
	if length > 1:
		for z in range(actual_start_z, end_z + 1):
			var key = str(start_x) + "," + str(z)
			used_positions[key] = true
	
	return {
		"length": length,
		"segment": {
			"material": material,
			"startPosition": {"x": start_x, "z": actual_start_z},
			"endPosition": {"x": start_x, "z": end_z}
		}
	}
