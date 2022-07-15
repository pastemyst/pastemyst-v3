package logging

import "go.uber.org/zap"

var Logger *zap.SugaredLogger

var prod *zap.Logger

// Initializes the logger
func InitLogger() error {
	log, err := zap.NewDevelopment()
	if err != nil {
		return err
	}

	prod = log

	Logger = prod.Sugar()

	return nil
}

// Closes the logger by flushing any remaining output.
func CloseLogger() {
	prod.Sync()
}
