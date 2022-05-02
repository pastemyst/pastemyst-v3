export default class TabData {
    id: number;
    title: string;
    isInRenamingState: boolean = false;
    editor: Editor;

    constructor(id: number, title: string, editor: Editor) {
        this.id = id;
        this.title = title;
        this.editor = editor;
    }
}
