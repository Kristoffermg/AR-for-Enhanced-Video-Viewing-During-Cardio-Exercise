import joblib
import pandas as pd
from pathlib import Path

current_dir = Path(__file__).resolve().parent

class InferenceHandler:
    def __init__(self):
        self.treadmill_intensity = joblib.load(current_dir / "pkl" / "treadmill_intensity.pkl")
        self.elliptical_intensity = joblib.load(current_dir / "pkl" / "elliptical_intensity.pkl")
        self.row_intensity = joblib.load(current_dir / "pkl" / "row_intensity.pkl")
        self.machine_classification = joblib.load(current_dir / "pkl" / "machine_classification_experiment1.pkl")

    def run(self, model: str, model_input):
        if model == "treadmill_intensity":
            prediction = self.treadmill_intensity.predict(model_input)
        elif model == "elliptical_intensity":
            prediction = self.elliptical_intensity.predict(model_input)
        elif model == "row_intensity":
            prediction = self.row_intensity.predict(model_input)
        elif model == "machine_classification":
            prediction = self.machine_classification.predict(model_input)
        else:
            raise ValueError(f"Model value does not exist: {model}")
        
        return prediction