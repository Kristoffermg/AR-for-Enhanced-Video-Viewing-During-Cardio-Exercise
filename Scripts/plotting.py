import pandas as pd
import matplotlib.pyplot as plt
import os

# Function to plot data from a single CSV
def plot_csv(ax, csv_file, title, bottom_label=False):
    data = pd.read_csv(csv_file)
    # Extract the first 600 rows (10 seconds of data)
    data = data.head(600)
    
    # Convert meters to centimeters for x, y, z
    data['x'] = data['x'] * 100
    data['y'] = data['y'] * 100
    data['z'] = data['z'] * 100
    time = data.index / 60.0

    ax.plot(time, data['x'], label='x (cm)', color='r')
    ax.plot(time, data['y'], label='y (cm)', color='g')
    ax.plot(time, data['z'], label='z (cm)', color='b')
    

    ax.set_title(title, fontsize=12)


csv_files = [
    "Data/Elliptical_peter.csv", "Data/Elliptical_kristoffer.csv",
    "Data/Row_peter.csv", "Data/Row_kristoffer.csv",
    "Data/Stair_peter.csv", "Data/Stair_kristoffer.csv",
    "Data/Walk_peter.csv", "Data/Walk_kristoffer.csv"
]

titles = [
    "Elliptical (user 1)", "Elliptical (user 2)",
    "Cardio Row Machine (user 1)", "Cardio Row Machine (user 2)",
    "Stairmaster (user 1)", "Stairmaster (user 2)",
    "Treadmill Walking (user 1)", "Treadmill Walking (user 2)"
]


fig, axs = plt.subplots(4, 2, figsize=(12, 8), sharex=True, sharey=True)
axs = axs.ravel()

for i, (csv_file, title) in enumerate(zip(csv_files, titles)):

    if os.path.exists(csv_file):
        plot_csv(axs[i], csv_file, title)
    else:
        axs[i].text(0.5, 0.5, "File not found", fontsize=12, ha='center')
        axs[i].set_title(title)

plt.tight_layout()
plt.show()





