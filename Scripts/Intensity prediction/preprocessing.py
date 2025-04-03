import pandas as pd
import numpy as np
import os
from tqdm import tqdm

TREADMILL_TRAINING_CSV = "treadmill_training.csv"
ELLIPTICAL_TRAINING_CSV = "elliptical_training.csv"
ROW_TRAINING_CSV = "row_training.csv"

class Preprocessing:
    def __init__(self, data_path, label=""):
        self.data_path = data_path
        self.label = label

    def preprocess(self):
        dataframe = pd.read_csv(self.data_path, usecols=('x', 'y', 'z'))
        mean = dataframe.mean(axis="index")
        standard_deviation = dataframe.std(axis="index")
        energy = (dataframe ** 2).sum()
        sum_of_absolute_differences = dataframe.diff().abs().sum()

        feature_vector = np.concatenate([mean, standard_deviation, energy, sum_of_absolute_differences])
        return feature_vector

    def save_to_csv(self, feature_vector, training_csv):
        new_data = pd.DataFrame([feature_vector], columns=[f'feature_{i}' for i in range(len(feature_vector))])
        new_data['label'] = self.label

        if not os.path.exists(training_csv):
            new_data.to_csv(training_csv, index=False)
        else:
            existing_data = pd.read_csv(training_csv)
            updated_data = pd.concat([existing_data, new_data], ignore_index=True)
            updated_data.to_csv(training_csv, index=False)

    def split_csv_into_multiple_files(self, chunk_size, machine):
        if not os.path.exists(self.data_path):
            raise FileNotFoundError(f"The file {csv_file} does not exist.")
        
        filename = os.path.basename(self.data_path).split('.')[0]
        folder_path = f"{os.path.dirname(self.data_path)}/{machine}/{filename}"
        if not os.path.exists(folder_path):
            os.makedirs(folder_path, exist_ok=True)

        df = pd.read_csv(self.data_path, chunksize=chunk_size)
        for i, chunk in enumerate(df):
            chunk.to_csv(f"{folder_path}/{filename}_chunk_{i*chunk_size}-{(i+1)*chunk_size}.csv", index=False)

def split_files_in_directory_into_chunks(directory, chunk_size=1000):
    for file in tqdm(os.listdir(directory), desc="Processing files"):
        if file.endswith(".csv"):
            data_path = os.path.join(directory, file)
            file_lower = file.lower()
            machine = ""
            if "treadmill" in file_lower:
                machine = "treadmill"
            elif "elliptical" in file_lower:
                machine = "elliptical"
            elif "row" in file_lower:
                machine = "row"
            else:
                print(":(")

            new_csv_path = f"{directory}/{machine}/{file}"
            if os.path.exists(new_csv_path):
                print(f"File already exists: {file}")
                continue

            preprocessing = Preprocessing(data_path)
            preprocessing.split_csv_into_multiple_files(chunk_size=1000, machine=machine)

        

def write_chunks_to_csv(folder_path, training_csv):
    label = ""
    folder_path_name_lower = os.path.basename(folder_path).lower()
    if "low" in folder_path_name_lower:
        label = "low"
    elif "medium" in folder_path_name_lower:
        label = "medium"
    elif "high" in folder_path_name_lower:
        label = "high"
    else:
        print("No label found in the file name.")
        return

    for file in os.listdir(folder_path):
        if file.endswith(".csv"):
            file_path = os.path.join(folder_path, file)
            preprocessing = Preprocessing(file_path, label)
            feature_vector = preprocessing.preprocess()
            preprocessing.save_to_csv(feature_vector, training_csv)            

if __name__ == "__main__":
    # split_files_in_directory_into_chunks(r"F:\GitHub\ARCH\Data\intensity\session", chunk_size=1000)
    write_chunks_to_csv(r"F:\GitHub\ARCH\Data\intensity\session\Elliptical\peterMEDIUMelliptical", ELLIPTICAL_TRAINING_CSV)

