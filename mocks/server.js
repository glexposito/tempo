import express from "express";

const app = express();
const port = 3000;

// GET /v1/search?name={city}&count=1
app.get("/v1/search", (req, res) => {
  const city = req.query.name ?? "";

  if (city === "nocity") {
    return res.json({ results: [] });
  }

  if (city === "noforecast") {
    return res.json({
      results: [{ name: "noforecast", latitude: 0, longitude: 0 }],
    });
  }

  const latitude = +(Math.random() * 180 - 90).toFixed(4);
  const longitude = +(Math.random() * 360 - 180).toFixed(4);

  res.json({
    results: [{ name: city, latitude, longitude }],
  });
});

// GET /v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m
app.get("/v1/forecast", (req, res) => {
  const lat = parseFloat(req.query.latitude);
  const lon = parseFloat(req.query.longitude);

  if (lat === 0 && lon === 0) {
    return res.status(500).json({ error: "simulated forecast failure" });
  }

  const temperature = +(Math.random() * 60 - 10).toFixed(1);

  res.json({
    current: { temperature_2m: temperature },
  });
});

app.listen(port, () => {
  console.log(`Mock server listening on port ${port}`);
});
