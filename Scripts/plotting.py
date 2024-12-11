import json
import matplotlib.pyplot as plt
with open('filemane.json', 'r') as file:
    data = json.load(file)

filtered_data = {key: value for key, value in data.items() if 600 <= int(key) <= 1800} # define window of json elements to be plotted

frames = sorted(filtered_data.keys(), key=lambda k: int(k))
times = [int(frame) / 60 for frame in frames]

times = [time - times[0] for time in times]

x_values = [filtered_data[frame]["x"] for frame in frames]
y_values = [filtered_data[frame]["y"] for frame in frames]
z_values = [filtered_data[frame]["z"] for frame in frames]

plt.figure(figsize=(12, 4))
# plt.plot(times, x_values, label="Side-to-side (x)", color="red", linewidth=1.5)
# plt.plot(times, y_values, label="Back-and-forth (y)", color="blue", linewidth=1.5)
# plt.plot(times, z_values, label="Up-and-down (z)", color="green", linewidth=1.5)
plt.plot(times, x_values, color="red", linewidth=1.5)
plt.plot(times, y_values, color="blue", linewidth=1.5)
plt.plot(times, z_values, color="green", linewidth=1.5)

plt.title("Head displacement for INSERT EXERCISE HERE (straight / downward)", fontsize=20)
# plt.xlabel("Time elapsed (seconds)", fontsize=20)
# plt.ylabel("Head displacement (meters)", fontsize=20)

plt.xticks(fontsize=16)
plt.yticks(fontsize=16)

plt.grid(True)
# plt.legend(fontsize=14, loc='lower right', frameon=True)
plt.show()
