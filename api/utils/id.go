package utils

import "math/rand"

var charRunes = []rune("abcdefghijklmnopqrstuvwxyz0123456789")

// Generates a random base36 8 character ID.
func RandomId() string {
	res := make([]rune, 8)
	for i := range res {
		res[i] = charRunes[rand.Intn(len(charRunes))]
	}

	return string(res)
}

// Keeps generating random base36 8 character IDs while the provided predicate function returns true.
//
// Usecase: generating IDs while the previous generated ID already exists in the DB.
func RandomIdWhile(pred func(string) bool) string {
	id := RandomId()
	for pred(id) {
		id = RandomId()
	}

	return id
}
