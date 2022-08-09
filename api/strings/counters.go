package strings

import (
	"bufio"
	"strings"
)

// Counts the number of lines of a string. Works with either \n or \r\n.
func CountLines(s string) (uint64, error) {
	return stringCounter(s, bufio.ScanLines)
}

// Counts the number of words of a string.
func CountWords(s string) (uint64, error) {
	return stringCounter(s, bufio.ScanWords)
}

// Counts occurances of anything in the string based on the provided SplitFunc.
func stringCounter(s string, splitFunc bufio.SplitFunc) (uint64, error) {
	scanner := bufio.NewScanner(strings.NewReader(s))

	scanner.Split(splitFunc)

	c := uint64(0)
	for scanner.Scan() {
		c++
	}

	if err := scanner.Err(); err != nil {
		return 0, err
	}

	return c, nil
}
