import type { Language } from "./api/lang";
import type { IndentUnit } from "./indentation";

export default class TabData {
    id: string = $state("");
    title: string = $state("untitled");
    content: string = $state("");
    cursorLine: number = $state(1);
    cursorCol: number = $state(0);
    language: Language | undefined = $state(undefined);
    indentationUnit: IndentUnit = $state("spaces");
    indentationWidth: number = $state(4);
    isInRenamingState = $state(false);

    constructor(id: string, title: string) {
        this.id = id;
        this.title = title;
    }
}
