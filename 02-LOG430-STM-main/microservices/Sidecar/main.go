package main

import (
	"fmt"
	"io"
	"log"
	"net/http"
	"os"

	"github.com/joho/godotenv"
)

func loadEnv() {
	err := godotenv.Load("/app/.env") // Charge les variables d'environnement
	if err != nil {
		log.Println("No .env file found, using system environment variables")
	}
}

func forwardRequest(w http.ResponseWriter, r *http.Request) {
	targetService := r.Header.Get("X-Target-Service")
	if targetService == "" {
		http.Error(w, "Missing X-Target-Service header", http.StatusBadRequest)
		return
	}

	targetURL := fmt.Sprintf("http://%s%s", targetService, r.URL.Path)
	log.Printf("Forwarding request to: %s", targetURL)

	req, err := http.NewRequest(r.Method, targetURL, r.Body)
	if err != nil {
		http.Error(w, "Failed to create request", http.StatusInternalServerError)
		return
	}
	req.Header = r.Header

	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		http.Error(w, "Request failed", http.StatusServiceUnavailable)
		return
	}
	defer resp.Body.Close()

	w.WriteHeader(resp.StatusCode)
	io.Copy(w, resp.Body)
}

func main() {
	loadEnv()

	port := os.Getenv("SIDECAR_PORT")
	if port == "" {
		port = "9090"
	}

	http.HandleFunc("/", forwardRequest)

	log.Printf("Sidecar running on port %s", port)
	log.Fatal(http.ListenAndServe(":"+port, nil))
}
