from fastapi import FastAPI
from pydantic import BaseModel
from fastapi.middleware.cors import CORSMiddleware
import pandas as pd
from typing import List

from models.inference import InferenceHandler

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

inferenceHandler = InferenceHandler()

class DataFrameRequest(BaseModel):
    feature_0: float
    feature_1: float
    feature_2: float
    feature_3: float
    feature_4: float
    feature_5: float
    feature_6: float
    feature_7: float
    feature_8: float
    feature_9: float
    feature_10: float
    feature_11: float

@app.get("/")
async def root():
    return {"message": "Hello World"}

@app.post("/run_inference")
async def inference(model: str, data: DataFrameRequest):
    input = pd.DataFrame({
        'feature_0': [data.feature_0],
        'feature_1': [data.feature_1],
        'feature_2': [data.feature_2],
        'feature_3': [data.feature_3],
        'feature_4': [data.feature_4],
        'feature_5': [data.feature_5],
        'feature_6': [data.feature_6],
        'feature_7': [data.feature_7],
        'feature_8': [data.feature_8],
        'feature_9': [data.feature_9],
        'feature_10': [data.feature_10],
        'feature_11': [data.feature_11]
    })
    prediction = inferenceHandler.run(model, input)
    return {"machine": prediction[0]}
