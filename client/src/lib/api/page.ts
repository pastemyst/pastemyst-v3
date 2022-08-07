export interface Page<T> {
    items: T[];
    totalPages: number;
    page: number;
    pageSize: number;
    hasNextPage: boolean;
}
