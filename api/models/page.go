package models

// Generic type for returning paginated results.
type Page[T any] struct {
	Items       []T   `json:"items"`
	TotalPages  int64 `json:"totalPages"`
	Page        int64 `json:"page"`
	PageSize    int64 `json:"pageSize"`
	HasNextPage bool  `json:"hasNextPage"`
}
