export interface Page<T> {
    items: T[];
    totalPages: number;
    currentPage: number;
    pageSize: number;
    hasNextPage: boolean;
}
