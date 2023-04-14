#include <stdio.h>

int single(int x);
int doubles(int x);
int triples(int x);
int quadtimes(int x);
int fivetimes(int x);

int main() {
	printf("%d x 1 = %d", 10, single(10));
	printf("%d x 2 = %d", 10, doubles(10));
	printf("%d x 3 = %d", 10, triples(10));
	printf("%d x 4 = %d", 10, quadtimes(10));
	printf("%d x 5 = %d", 10, fivetimes(10));
	return 0;
}