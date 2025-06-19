import pandas as pd
import json
import csv
from pprint import pprint

def read_data(json_path):
    with open(json_path, "r") as file:
        data = json.load(file)
    
    x_values = []
    y_values = []
    z_values = []
    for frame in data:
        x_values.append(data[frame]["x"])
        y_values.append(data[frame]["y"])
        z_values.append(data[frame]["z"])
    frames = list(range(1, len(x_values)+1))
    return (x_values, y_values, z_values, frames)


def json_to_dataframe(json_path):
    x_values, y_values, z_values, frames = read_data(json_path)
    data = {
        "X": {},
        "Y": {},
        "Z": {}
    }
    for i in frames:
        data["X"][i] = x_values[i-1]
        data["Y"][i] = y_values[i-1]
        data["Z"][i] = z_values[i-1]
    df = pd.DataFrame(data)

    return df

def json_to_csv(json_path, csv_path):
    x_values, y_values, z_values, frames = read_data(json_path)

    data = [
        ["frame", "x", "y", "z"]
    ]

    for i in range(len(frames)):
        data.append([i+1, x_values[i], y_values[i], z_values[i]])

    with open(csv_path, mode="w", newline="") as csv_file:
        writer = csv.writer(csv_file)
        writer.writerows(data)