# pastemyst contributing guide

this is a guide on how to contribute to the pastemyst project.

## issues

if you have a new feature suggestion, you noticed a bug or have documentation suggestions, first start with searching the issues. there is a chance the issue already exists.

if the issue doesn't exist, you can go ahead and create one.

there exists a couple of issue templates, depending on what type of issue you are creating.

please be patient in getting a response on your issues :)

## brancing

all branches follow a style guide: `type/issue-number/description`.

there are different branch types depending on the thing you are working on:
* feature
* fix
* docs

descriptions should be very short, but still make sense what the branch is about.

example branch names:
* `feature/15/encrypting-pastes`
* `fix/16/button-hover`
* `docs/17/get-paste`

## commits

like branches, commit messages should also follow a style guide: `type(issue-number): message`

types:
* feat
* fix
* docs

messages should be short but very descriptive. if a longer description is needed add it on a new line (with an empty line in between). the first line should be 50 characters or less (if possible), and the extra description can be 72 characters or less.

here's a good article on [writing git commit messages](https://cbea.ms/git-commit/).

example commit:

```
feat(174): added /ping route

added a /ping route that always returns 200 OK, used for testing
the server health.
```

## pull requests

once you think you are finished with a task, you can open a pull request (you can also open it earlier, but mark it as a draft).

github actions will run checks on your code, lint, build and test it. these are the first changes you will have to make (in case something fails).

the pr will get reviewed (and most likely commented on), after you fix all suggestions (discussions on suggestion are of course always welcome, and sometimes required), you can request another review.
