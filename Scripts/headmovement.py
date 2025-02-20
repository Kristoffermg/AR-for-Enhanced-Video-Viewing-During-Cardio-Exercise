import os
import json
import csv
import matplotlib.pyplot as plt
import numpy as np
from pprint import pprint
from sklearn.metrics import mean_absolute_error as mae, mean_squared_error as mse
import time
from scipy.signal import find_peaks, savgol_filter, medfilt

from dataconverter import json_to_dataframe
from dataconverter import json_to_csv

class HeadMovement:
    def __init__(self, data_path):
        if data_path.endswith(".json"):
            self.x_values, self.y_values, self.z_values, self.combined_values, self.frames = self.read_json_data(data_path)
            self.dataframe = json_to_dataframe(data_path)
        else:
            self.x_values, self.y_values, self.z_values, self.frames = self.read_csv_data(data_path)
        
        self.frames_length = self.frames[-1]

        csv_path = data_path.replace(".json", ".csv")
        if not os.path.exists(csv_path):
            json_to_csv(data_path, csv_path)
        self.csv_path = csv_path

    @staticmethod
    def read_json_data(json_path):
        with open(json_path, "r") as file:
            data = json.load(file)
        
        x_values = []
        y_values = []
        z_values = []
        combined_values = []
        for frame in data:
            x = data[frame]["x"]
            y = data[frame]["y"]
            z = data[frame]["z"]

            x_values.append(x)
            y_values.append(y)
            z_values.append(z)
            combined_values.append([x, y, z])
        frames = list(range(1, len(x_values)+1))
        return (x_values, y_values, z_values, np.array(combined_values), frames)
    
    @staticmethod
    def read_csv_data(csv_path):
        x_values = []
        y_values = []
        z_values = []
        with open(csv_path, mode="r") as csv_file:
            csvFile = csv.reader(csv_file)
            next(csvFile, None)
            for line in csvFile:
                x_values.append(float(line[1]))
                y_values.append(float(line[2]))
                z_values.append(float(line[3]))
        frames = list(range(1, len(x_values)+1))
        return (x_values, y_values, z_values, frames)

    def plot_sharing_axes(self, suptitle):
        plt.subplot(3, 1, 1)
        plt.title("X values")
        plt.plot(self.frames, self.x_values)

        plt.subplot(3, 1, 2)
        plt.title("Y values")
        plt.plot(self.frames, self.y_values)

        plt.subplot(3, 1, 3)
        plt.title("Z values")
        plt.plot(self.frames, self.z_values)

        plt.tight_layout()
        plt.suptitle(suptitle)

        plt.show()

    def head_movement_estimation(self, signal_data, window_size, estimation_size):
        """Estimates head movement with savgol filtering and signal peak detection

        Savgol filtering: Denoises the data by removing small peaks that dont represent a fill signal cycle 

        Arguments:
            window_size - The number of frames used to "train" for the estimation
            estimation_size - The number of frames to estimate ahead of the window
        """
        coordinate_value = self.x_values
        for window_position in range(600 + window_size, self.frames_length, estimation_size):
            test_data = np.array(coordinate_value[window_position:window_position+estimation_size])
            start_time = time.time()
            window_data = np.array(coordinate_value[window_position-window_size:window_position])

            window_data = medfilt(window_data, kernel_size=3)
            window_data = savgol_filter(window_data, window_length=11, polyorder=2)
            peaks = find_peaks(window_data)[0]

            # plt.plot(window_data)
            # plt.scatter(peaks, window_data[peaks], color="red")
            # plt.show()

            if len(peaks) == 0:
                print("No peaks found in the provided signal data")
                continue

            # Computes the average number of frames between the signal peaks
            cycle_steps = [peaks[0]]
            for peak_index in range(1, len(peaks)):
                cycle_steps.append(abs(peaks[peak_index] - peaks[peak_index-1]))
            average_cycle_steps =  round(sum(cycle_steps) / len(cycle_steps))

            # Splits the data between the signal peaks into arrays. These are called "cycles"
            cycles = []
            current_index = 0
            for cycle_step in cycle_steps:
                current_cycle = []
                for cycle_index in range(current_index, current_index+cycle_step):
                    current_cycle.append(window_data[cycle_index])
                cycles.append(current_cycle)
                current_index = current_index + cycle_step

            # The number of frames between the last peak and the end of the window_array
            remaining_frames = len(window_data) - (peaks[-1] - 1)

            last_peak_value = window_data[peaks[-1]+1]

            # Creates a single array that represents the average of all the cycles
            # average_cycle = []
            # for cycle_step in range(average_cycle_steps):
            #     current_cycle_step_values = []
            #     for cycle in cycles:
            #         if cycle_step >= len(cycle):
            #             continue
            #         current_cycle_step_values.append(cycle[cycle_step])
            #     average_cycle.append(sum(current_cycle_step_values) / len(current_cycle_step_values))

            average_cycle = []
            for cycle_step in range(average_cycle_steps):
                current_cycle_step_values = [
                    cycle[cycle_step] for cycle in cycles if cycle_step < len(cycle)
                ]
                # Assign higher weights to more recent cycles
                weights = np.linspace(1, len(current_cycle_step_values), len(current_cycle_step_values))
                weighted_average = np.average(current_cycle_step_values, weights=weights)
                average_cycle.append(weighted_average)

            # An array of the difference between an average cycles peak and the frames in the cycle
            relative_difference_to_peak = []
            for cycle_step in range(1, average_cycle_steps):
                relative_difference_to_peak.append(average_cycle[0] - average_cycle[cycle_step])
            
            # cycle_x allows an estimation size that is longer than the average cycles 
            estimations = []
            for current_estimation_index in range(estimation_size + remaining_frames - 1):
                cycle_x = current_estimation_index % len(average_cycle)
                estimations.append(last_peak_value-relative_difference_to_peak[cycle_x-1])

            end_time = time.time()

            print("Time elapsed:", end_time - start_time)

            estimations.insert(0, last_peak_value)
            plt.plot(self.frames[window_position-window_size:window_position], window_data, c='b', label="window")
            plt.plot(self.frames[window_position:window_position+estimation_size], test_data, c='r', label="gt")
            plt.plot(self.frames[window_position-remaining_frames:window_position+estimation_size], estimations, c="green", label="estimations")
            plt.title("yay")
            plt.show()

    def calculate_perceived_intensity(self):
        head_position_data = self.x_values[600:1200]

        head_position_data = medfilt(head_position_data, kernel_size=3)
        head_position_data = savgol_filter(head_position_data, window_length=11, polyorder=2)
        peaks = find_peaks(head_position_data)[0]

        previous_peak = peaks[0]
        accumulated_total_peak_difference = 0
        for i in range(1, len(peaks)):
            current_peak = peaks[i]
            accumulated_total_peak_difference += current_peak - previous_peak
            previous_peak = current_peak
        
        print(accumulated_total_peak_difference / len(peaks)-1)


        

        



    
if __name__ == "__main__":
    headMovement = HeadMovement(r"c:\Users\test\Documents\Data\headPositionDataFilterMedMovement.csv")
    # headMovement = HeadMovement(r"Run_2.csv")
    headMovement.frames = headMovement.frames[30:len(headMovement.frames)-30]
    headMovement.x_values = headMovement.x_values[30:len(headMovement.x_values)-30]
    headMovement.y_values = headMovement.y_values[30:len(headMovement.y_values)-30]
    headMovement.z_values = headMovement.z_values[30:len(headMovement.z_values)-30]
    headMovement.plot_sharing_axes("pik")
    # headMovement.head_movement_estimation(window_size=2000, estimation_size=60)
    # headMovement.calculate_perceived_intensity()
