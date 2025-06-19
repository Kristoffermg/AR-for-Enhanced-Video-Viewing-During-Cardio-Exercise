import pandas as pd
import matplotlib.pyplot as plt
import os

# Function to plot data from a single CSV
def plot_csv(ax, csv_file, title, bottom_label=False):
    data = pd.read_csv(csv_file)
    # Extract the first 600 rows (10 seconds of data)
    data = data.head(720)
    
    # adjusted data
    if csv_file == r'Data\intensity\session\krisLOWrow.csv':
        data['x'] = (data['x']-1.19095367833664) * 100
        data['y'] = (data['y']-0.0200795717053383) * 100
        data['z'] = (data['z']-0.277819227419859) * 100
        time = data.index / 72.0
    elif csv_file == r'Data\intensity\session\krisMEDrow.csv':
        data['x'] = (data['x']-1.22830774418456) * 100
        data['y'] = (data['y']-0.01233788914236) * 100
        data['z'] = (data['z']-0.263460008204631) * 100
        time = data.index / 72.0
    else :
        data['x'] = (data['x']-1.19651592939548) * 100
        data['y'] = (data['y']-0.0195061026514433) * 100
        data['z'] = (data['z']-0.352275000637558) * 100
        time = data.index / 72.0

    # data['x'] = (data['x']) * 100
    # data['y'] = (data['y']) * 100
    # data['z'] = (data['z']) * 100
    # time = data.index / 60.0

    ax.plot(time, data['x'], label='x (cm)', color='r')
    ax.plot(time, data['y'], label='y (cm)', color='g')
    ax.plot(time, data['z'], label='z (cm)', color='b')
    
    ax.set_title(title, fontsize=12)


csv_files = [
    r'Data\intensity\session\krisLOWrow.csv',
    r'Data\intensity\session\krisMEDrow.csv',
    r'Data\intensity\session\krisHIGHrow.csv'

    # 'Data/intensity/elliptical/low_elliptical.csv',
    # 'Data/intensity/elliptical/med_elliptical.csv',
    # 'Data/intensity/elliptical/high_elliptical.csv'

    # 'Data/intensity/row/low_row.csv',
    # 'Data/intensity/row/med_row.csv',
    # 'Data/intensity/row/high_row.csv'
]

titles = [
    "Row Low",
    "Row Medium",
    "Row High"

    # "Ellipcal low intensity",
    # "Elliptical medium intensity",
    # "Elliptical high intensity"

    # "Row low intensity",
    # "Row medium intensity",
    # "Row high intensity"
]


fig, axs = plt.subplots(3, 1, figsize=(8, 6), sharex=True, sharey=True)
axs = axs.ravel()

for i, (csv_file, title) in enumerate(zip(csv_files, titles)):
    if os.path.exists(csv_file):
        plot_csv(axs[i], csv_file, title)
    else:
        axs[i].text(0.5, 0.5, "File not found", fontsize=12, ha='center')
        axs[i].set_title(title)
# plt.legend()
plt.tight_layout()
plt.show()



# import pandas as pd
# import matplotlib.pyplot as plt

# def plot_intensity(file_path):
#     df = pd.read_csv(file_path)

#     # Extract frame numbers and x, y, z values
#     frames = df["frame"]
#     x_values = df["x"] * 100
#     y_values = df["y"] * 100
#     z_values = df["z"] * 100

#     # Create a figure for a single plot
#     plt.figure(figsize=(10, 6))

#     # Plot X, Y, and Z values on the same plot
#     plt.plot(frames, x_values, color="r", label="X values")
#     plt.plot(frames, y_values, color="g", label="Y values")
#     plt.plot(frames, z_values, color="b", label="Z values")

#     # Adding labels, legend, and grid
#     plt.xlabel("Frame")
#     plt.ylabel("Values")
#     plt.legend()
#     plt.grid(True)

#     # Show the plot
#     plt.tight_layout()
#     plt.show()

# # Load the CSV file
# file_path = "Data/intensity/treadmill/4.5speed_treadmill.csv"  # Update with your actual file path
# plot_intensity(file_path)

