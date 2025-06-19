import pandas as pd
import numpy as np
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import accuracy_score

training_csv = r"C:\Users\test\Documents\GitHub\ARCH\Scripts\Machine learning\treadmill_training.csv"

data = pd.read_csv(training_csv).drop(columns=['Unnamed: 0'], errors='ignore')

X = data.drop(columns='label') 
y = data['label'] 

X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

rf_model = RandomForestClassifier(n_estimators=100, random_state=42)

rf_model.fit(X_train, y_train)

y_pred = rf_model.predict(X_test)

accuracy = accuracy_score(y_test, y_pred)
print(f"Accuracy: {accuracy * 100:.2f}%")

comparison = pd.DataFrame({'Actual': y_test.values, 'Predicted': y_pred})
print(comparison)


import joblib
joblib.dump(rf_model, 'treadmill_intensity.pkl')
