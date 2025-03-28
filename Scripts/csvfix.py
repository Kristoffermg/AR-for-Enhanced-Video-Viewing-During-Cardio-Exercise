import csv

def rename_frames(input_file, output_file):
    with open(input_file, 'r', newline='') as infile:
        reader = list(csv.reader(infile))
        header = reader[0]  # Keep the header
        data = reader[1:]   # Exclude the header
        
        if not data:
            print("Error: No data found in the file.")
            return
        
        
        # Adjust the frame numbers to start from 1
        for i, row in enumerate(data):
            row[0] = str(i + 1)
        
    with open(output_file, 'w', newline='') as outfile:
        writer = csv.writer(outfile)
        writer.writerow(header)  # Write the header
        writer.writerows(data)   # Write the modified data

    print(f"Successfully renamed frames in '{input_file}' and saved to '{output_file}'")




def fix_file(file_path):
    with open(file_path, "r") as f:
        lines = f.readlines()

    # Process each line to replace the misplaced commas
    fixed_lines = []
    for line in lines:
        parts = line.strip().split(",")
        
        if parts[0] == "frame":  # Keep header as is
            fixed_lines.append(line.strip())
        else:
            corrected_values = [parts[0]]  # Keep frame number unchanged
            for i in range(1, len(parts), 2):  # Merge every two parts into a correct float
                corrected_values.append(f"{parts[i]}.{parts[i+1]}")
            fixed_lines.append(",".join(corrected_values))

    # Save the corrected data
    fixed_file_path = file_path
    with open(fixed_file_path, "w") as f:
        f.write("\n".join(fixed_lines))

    print(f"Fixed file saved as {fixed_file_path}")

file_to_fix = "Data/intensity/row/med_row.csv"  # Update with your actual file path
# fix_file(file_to_fix)


input_path =  r'Data\intensity\treadmill\peter\high10.csv'
output_path = r'Data\intensity\treadmill\peter\high10.csv'
rename_frames(input_path, output_path)