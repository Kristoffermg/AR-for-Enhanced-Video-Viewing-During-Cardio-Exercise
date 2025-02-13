# import pandas as pd
# import matplotlib.pyplot as plt
# import os

# # Function to plot data from a single CSV
# def plot_csv(ax, csv_file, title, bottom_label=False):
#     data = pd.read_csv(csv_file)
#     # Extract the first 600 rows (10 seconds of data)
#     data = data.head(600)
    
#     # Convert meters to centimeters for x, y, z
#     data['x'] = data['x'] * 100
#     data['y'] = data['y'] * 100
#     data['z'] = data['z'] * 100
#     time = data.index / 60.0

#     ax.plot(time, data['x'], label='x (cm)', color='r')
#     ax.plot(time, data['y'], label='y (cm)', color='g')
#     ax.plot(time, data['z'], label='z (cm)', color='b')
    

#     ax.set_title(title, fontsize=12)


# csv_files = [
#     'Data/intensity/row/low_row.csv'
# ]

# titles = [
#     "low"
# ]


# fig, axs = plt.subplots(2, 1, figsize=(12, 8), sharex=True, sharey=True)
# axs = axs.ravel()

# for i, (csv_file, title) in enumerate(zip(csv_files, titles)):

#     if os.path.exists(csv_file):
#         plot_csv(axs[i], csv_file, title)
#     else:
#         axs[i].text(0.5, 0.5, "File not found", fontsize=12, ha='center')
#         axs[i].set_title(title)

# plt.tight_layout()
# plt.show()



import pandas as pd
import matplotlib.pyplot as plt

# Load the fixed CSV file
file_path = "Data/intensity/row/low_row.csv"  # Update with your actual file path
def plot_intensity (file_path):
    df = pd.read_csv(file_path)

    # Extract frame numbers and x, y, z values
    frames = df["frame"]
    x_values = df["x"]
    y_values = df["y"]
    z_values = df["z"]

    # Create a figure with three subplots
    plt.figure(figsize=(10, 6))

    # Plot X values
    plt.subplot(3, 1, 1)
    plt.plot(frames, x_values, marker="o", linestyle="-", color="r", label="X values")
    plt.xlabel("Frame")
    plt.ylabel("X")
    plt.legend()
    plt.grid()

    # Plot Y values
    plt.subplot(3, 1, 2)
    plt.plot(frames, y_values, marker="o", linestyle="-", color="g", label="Y values")
    plt.xlabel("Frame")
    plt.ylabel("Y")
    plt.legend()
    plt.grid()

    # Plot Z values
    plt.subplot(3, 1, 3)
    plt.plot(frames, z_values, marker="o", linestyle="-", color="b", label="Z values")
    plt.xlabel("Frame")
    plt.ylabel("Z")
    plt.legend()
    plt.grid()

    # Adjust layout and show plot
    plt.tight_layout()
    plt.show()




