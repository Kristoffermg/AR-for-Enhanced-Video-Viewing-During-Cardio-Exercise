import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
from filterpy.kalman import UnscentedKalmanFilter as UKF, MerweScaledSigmaPoints


# Load data
data = pd.read_csv('Data/Elliptical_1.csv')
time_step = 1 / 60  # Each frame is 1/60th of a second

# State dimensions: [x, y, z, vx, vy, vz]
state_dim = 6
measurement_dim = 3

# Nonlinear state transition function
def fx(state, dt):
    """
    State transition function. Assumes oscillatory motion for simplicity.
    State = [x, y, z, vx, vy, vz]
    """
    x, y, z, vx, vy, vz = state
    x += vx * dt
    y += vy * dt
    z += vz * dt
    return np.array([x, y, z, vx, vy, vz])

# Measurement function (linear, maps state to observed x, y, z)
def hx(state):
    """
    Measurement function. Returns observed positions [x, y, z].
    """
    return state[:3]

# Create Unscented Kalman Filter
points = MerweScaledSigmaPoints(n=state_dim, alpha=0.1, beta=2.0, kappa=0)
ukf = UKF(dim_x=state_dim, dim_z=measurement_dim, fx=fx, hx=hx, dt=time_step, points=points)

# Initialize UKF parameters
ukf.x = np.zeros(state_dim)  # Initial state
ukf.P *= 1.0  # State covariance
ukf.Q = np.eye(state_dim) * 0.01  # Process noise covariance
ukf.R = np.eye(measurement_dim) * 0.05  # Measurement noise covariance

# Apply UKF to the data
predictions = []
for _, row in data.iterrows():
    measurement = np.array([row['x'], row['y'], row['z']])
    ukf.predict()
    ukf.update(measurement)
    predictions.append(ukf.x[:3])  # Save predicted positions

# Convert predictions to DataFrame
predicted_df = pd.DataFrame(predictions, columns=['x_pred', 'y_pred', 'z_pred'])
predicted_df.to_csv('predictions.csv', index=False)

# Calculate errors between actual and predicted positions
errors = {
    'frame': data['frame'],
    'x_error': np.abs(data['x'] - predicted_df['x_pred']),
    'y_error': np.abs(data['y'] - predicted_df['y_pred']),
    'z_error': np.abs(data['z'] - predicted_df['z_pred'])
}

# Convert to DataFrame
errors_df = pd.DataFrame(errors)

# Calculate summary statistics
x_mae = errors_df['x_error'].mean()
y_mae = errors_df['y_error'].mean()
z_mae = errors_df['z_error'].mean()

x_rmse = np.sqrt(np.mean(errors_df['x_error']**2))
y_rmse = np.sqrt(np.mean(errors_df['y_error']**2))
z_rmse = np.sqrt(np.mean(errors_df['z_error']**2))

print("Summary of Errors:")
print(f"MAE (x): {x_mae:.6f}, RMSE (x): {x_rmse:.6f}")
print(f"MAE (y): {y_mae:.6f}, RMSE (y): {y_rmse:.6f}")
print(f"MAE (z): {z_mae:.6f}, RMSE (z): {z_rmse:.6f}")

# Save frame-by-frame errors to CSV
errors_df.to_csv('errors.csv', index=False)
print("Errors saved to 'errors.csv'.")

# Plot actual vs predicted data
actual_positions = data[['x', 'y', 'z']].values
predicted_positions = np.array(predictions)

plt.figure(figsize=(15, 5))

for i, coord in enumerate(['x', 'y', 'z']):
    plt.subplot(1, 3, i + 1)
    plt.plot(actual_positions[:, i], label=f'Actual {coord}', color='red')
    plt.plot(predicted_positions[:, i], label=f'Predicted {coord}', linestyle='--', color='blue')
    plt.xlabel('Frame')
    plt.ylabel(f'{coord}-coordinate')
    plt.title(f'Actual vs Predicted {coord}')
    plt.legend()

plt.tight_layout()
plt.show()

# Plot errors
plt.figure(figsize=(12, 6))
plt.plot(errors_df['frame'], errors_df['x_error'], label='X Error', color='red')
plt.plot(errors_df['frame'], errors_df['y_error'], label='Y Error', color='green')
plt.plot(errors_df['frame'], errors_df['z_error'], label='Z Error', color='blue')
plt.xlabel('Frame')
plt.ylabel('Absolute Error')
plt.title('Prediction Errors for X, Y, Z Coordinates')
plt.legend()
plt.show()

print("Predictions saved to 'predictions.csv'.")
