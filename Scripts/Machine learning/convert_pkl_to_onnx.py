import joblib
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType

model = joblib.load("models/machine_classification.pkl") 

initial_type = [("input", FloatTensorType([None, 4]))]

onnx_model = convert_sklearn(model, initial_types=initial_type)
with open("machine_classification.onnx", "wb") as f:
    f.write(onnx_model.SerializeToString())
