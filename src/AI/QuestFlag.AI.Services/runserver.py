import os
from pathlib import Path

from fastapi import FastAPI, Request, Form
from fastapi.responses import HTMLResponse
from fastapi.templating import Jinja2Templates

app = FastAPI(title="FastAPI Demo")

templates = Jinja2Templates(directory=str(Path(__file__).resolve().parent / "templates"))

# Home page
@app.get("/", response_class=HTMLResponse)
async def home(request: Request):
    return templates.TemplateResponse(
        "index.html",
        {"request": request, "message": ""}
    )

# Handle form submission
@app.post("/submit", response_class=HTMLResponse)
async def submit(request: Request, name: str = Form(...)):
    message = f"Hello {name}, FastAPI received your input!"
    return templates.TemplateResponse(
        "index.html",
        {"request": request, "message": message}
    )

# JSON API endpoint
@app.get("/api/users")
async def get_users():
    return [
        {"id": 1, "name": "Rudra"},
        {"id": 2, "name": "Admin"}
    ]