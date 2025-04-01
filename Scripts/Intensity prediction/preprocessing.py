import pandas as pd
import numpy as np
import os

training_csv = r"F:\GitHub\scripts\Classification for head data\training.csv"

dataframe = pd.read_csv(r"F:\GitHub\ARCH\Data\intensity\treadmill\peter\high10.csv", usecols=('x', 'y', 'z'))
label = "high"

mean = dataframe.mean(axis="index")
standard_deviation = dataframe.std(axis="index")
energy = (dataframe ** 2).sum()
sum_of_absolute_differences = dataframe.diff().abs().sum()

feature_vector = np.concatenate([mean, standard_deviation, energy, sum_of_absolute_differences])

new_data = pd.DataFrame([feature_vector], columns=[f'feature_{i}' for i in range(len(feature_vector))])
new_data['label'] = label

if not os.path.exists(training_csv):
    new_data.to_csv(training_csv, index=False)

existing_data = pd.read_csv(training_csv)
updated_data = pd.concat([existing_data, new_data], ignore_index=True)
updated_data.to_csv(training_csv, index=False)
