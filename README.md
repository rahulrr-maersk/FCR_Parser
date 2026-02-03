# FCR Parser

Extracts data from FCR (Freight Cargo Receipt) CSV files.

## What It Does

Extracts three things from FCR CSV files:
1. **Shipper Name & Address** (using AI)
2. **Marks and Numbers** (direct extraction)
3. **Cargo Description** (direct extraction)

## Quick Start

```bash
# 1. Setup
dotnet restore
dotnet user-secrets set "AI:GroqKey" "your-api-key"

# 2. Run
# Place CSV files in FcrParser/Input/
dotnet run --project FcrParser

# 3. Get results from FcrParser/Output/
```

## Project Structure

```
FcrParser_root/                 # Solution root
├── FcrParser/                  # Main project
│   ├── Input/                  # Place CSV files here
│   ├── Output/                 # JSON and TXT outputs
│   ├── Services/               # Extraction logic
│   ├── Models/                 # Data models
│   └── Program.cs              # Entry point
│
├── FcrParser.Tests/           # Unit tests
└── docs/                       # Documentation
```

## How It Works

```
CSV File → Parser → JSON + TXT Output
           ├─ Direct extraction (Marks & Cargo)
           └─ AI extraction (Shipper info)
```

## AI Providers

The tool is modular - use any AI provider. Currently tested:
- Groq (Llama 3.1)
- Cerebras (Llama 3.1)
- DeepSeek
- Mistral
- Gemini

If one provider fails (rate limit, etc.), it automatically tries the next one.

## Commands

```bash
# Build
dotnet build

# Test
dotnet test

# Run
dotnet run --project FcrParser
```

## Output Example

**JSON:**
```json
{
  "MarksAndNumbers": ["MSKU1234567"],
  "CargoDescription": ["ELECTRONIC COMPONENTS"],
  "ShipperInfo": {
    "Name": "ABC Electronics Ltd.",
    "Address": "123 Industrial Zone, Shenzhen, China"
  }
}
```

**TXT:**
```
Marks And Numbers:
MSKU1234567

Cargo Description:
ELECTRONIC COMPONENTS

Shipper Info:
  Name: ABC Electronics Ltd.
  Address: 123 Industrial Zone, Shenzhen, China
```

## Documentation

See `docs/FCR_Parser_Confluence_Documentation.md` for detailed documentation.

---

Simple, reliable FCR data extraction.
