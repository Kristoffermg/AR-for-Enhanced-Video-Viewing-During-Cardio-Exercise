import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
from collections import deque

# Load the CSV file
data = pd.read_csv('Data/Elliptical_1.csv')
time_step = 1 / 60  # Each frame is 1/60th of a second

# Initialize Kalman filter parameters
state_dim = 6  # [x, y, z, vx, vy, vz]
measurement_dim = 3  # [x, y, z]

# State vector: [x, y, z, vx, vy, vz]
state = np.zeros((state_dim, 1))

# State transition matrix (A)
A = np.eye(state_dim)
for i in range(3):
    A[i, i + 3] = time_step

# Measurement matrix (H): Maps state to measurement space
H = np.zeros((measurement_dim, state_dim))
H[:3, :3] = np.eye(3)

# Process noise covariance (Q): Assumes small acceleration noise
q = 0.01
Q = q * np.eye(state_dim)

# Measurement noise covariance (R): Assumes measurement noise
r = 0.05
R = r * np.eye(measurement_dim)

# Initial estimate error covariance (P)
P = np.eye(state_dim)

# Sliding window parameters
window_size = 10  # Number of recent measurements to keep
sliding_window = deque(maxlen=window_size)

# Function to predict the next state
def predict(state, P):
    state_pred = A @ state
    P_pred = A @ P @ A.T + Q
    return state_pred, P_pred

# Function to update the state with a new measurement
def update(state_pred, P_pred, measurement):
    y = measurement.reshape(-1, 1) - (H @ state_pred)  # Measurement residual
    S = H @ P_pred @ H.T + R  # Residual covariance
    K = P_pred @ H.T @ np.linalg.inv(S)  # Kalman gain
    state_updated = state_pred + K @ y
    P_updated = (np.eye(state_dim) - K @ H) @ P_pred
    return state_updated, P_updated

# Function to incorporate sliding window into state updates
def refine_with_sliding_window(window):
    if len(window) < 2:
        return state  # Not enough data to refine

    # Calculate velocity estimates from the sliding window
    recent_positions = np.array(window)
    velocities = np.diff(recent_positions, axis=0) / time_step
    avg_velocity = velocities.mean(axis=0)

    # Update state directly with refined velocity
    state[:3] = recent_positions[-1].reshape(-1, 1)  # Update position
    state[3:] = avg_velocity.reshape(-1, 1)  # Update velocity
    return state

# Apply Kalman filter to the data
predictions = []
for _, row in data.iterrows():
    measurement = np.array([row['x'], row['y'], row['z']])
    sliding_window.append(measurement)  # Add to sliding window

    # Refine state using sliding window
    state = refine_with_sliding_window(sliding_window)

    # Predict
    state, P = predict(state, P)
    # Update
    state, P = update(state, P, measurement)
    predictions.append(state[:3].flatten())

# Save results to CSV
predicted_df = pd.DataFrame(predictions, columns=['x_pred', 'y_pred', 'z_pred'])
predicted_df.to_csv('predictions.csv', index=False)

# Plot the actual and predicted values
actual_positions = data[['x', 'y', 'z']].values
predicted_positions = np.array(predictions)

plt.figure(figsize=(15, 5))

for i, coord in enumerate(['x', 'y', 'z']):
    plt.subplot(1, 3, i + 1)
    plt.plot(actual_positions[:, i], label=f'Actual {coord}')
    plt.plot(predicted_positions[:, i], label=f'Predicted {coord}', linestyle='--')
    plt.xlabel('Frame')
    plt.ylabel(f'{coord}-coordinate')
    plt.title(f'Actual vs Predicted {coord}')
    plt.legend()

plt.tight_layout()
plt.show()

print("Predictions saved to 'predictions.csv'.")
